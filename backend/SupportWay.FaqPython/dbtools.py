from __future__ import annotations

import os
from dataclasses import dataclass
from datetime import datetime
from typing import Any
from urllib.parse import quote_plus

from sqlalchemy import create_engine, text
from sqlalchemy.engine import Engine
from sqlalchemy.exc import SQLAlchemyError


DEFAULT_NPGSQL_CONNECTION_STRING = (
    "Host=localhost;Port=5432;Database=SupportWay;Username=postgres;Password=admin"
)


@dataclass(frozen=True)
class DbHealth:
    status: str
    detail: str | None = None


def _get_raw_connection_string() -> str:
    return (
        os.getenv("SUPPORTWAY_DB_CONNECTION_STRING")
        or os.getenv("POSTGRES_CONNECTION_STRING")
        or os.getenv("DATABASE_URL")
        or DEFAULT_NPGSQL_CONNECTION_STRING
    ).strip()


def _parse_semicolon_connection_string(raw: str) -> dict[str, str]:
    result: dict[str, str] = {}
    for part in raw.split(";"):
        part = part.strip()
        if not part or "=" not in part:
            continue
        key, value = part.split("=", 1)
        result[key.strip().lower()] = value.strip()
    return result


def _to_sqlalchemy_url(raw: str) -> str:
    """Accepts SQLAlchemy URL, PostgreSQL URL, or .NET/Npgsql style string."""
    if not raw:
        raise ValueError("Database connection string is empty")

    if raw.startswith("postgresql+psycopg2://"):
        return raw

    if raw.startswith("postgresql://") or raw.startswith("postgres://"):
        if raw.startswith("postgres://"):
            raw = raw.replace("postgres://", "postgresql://", 1)
        return raw.replace("postgresql://", "postgresql+psycopg2://", 1)

    parts = _parse_semicolon_connection_string(raw)

    host = parts.get("host") or parts.get("server") or "localhost"
    port = parts.get("port") or "5432"
    database = parts.get("database") or parts.get("dbname") or "SupportWay"
    username = parts.get("username") or parts.get("user id") or parts.get("userid") or parts.get("user") or "postgres"
    password = parts.get("password") or parts.get("pwd") or ""

    user_part = quote_plus(username)
    password_part = f":{quote_plus(password)}" if password else ""
    return f"postgresql+psycopg2://{user_part}{password_part}@{host}:{port}/{quote_plus(database)}"


def build_engine() -> Engine:
    raw = _get_raw_connection_string()
    return create_engine(
        _to_sqlalchemy_url(raw),
        pool_pre_ping=True,
        pool_recycle=1800,
        future=True,
        echo=False,
        connect_args={"connect_timeout": 8},
    )


class SupportWayDataTools:
    """Read-only analytics helpers for the real SupportWay PostgreSQL database."""

    def __init__(self, engine: Engine):
        self.engine = engine

    @staticmethod
    def _safe_datetime_to_str(value: Any) -> str | None:
        if value is None:
            return None
        if isinstance(value, datetime):
            return value.isoformat()
        return str(value)

    def health(self) -> DbHealth:
        try:
            with self.engine.connect() as conn:
                conn.execute(text("SELECT 1"))
            return DbHealth(status="ok")
        except SQLAlchemyError as exc:
            return DbHealth(status="error", detail=str(exc))
        except Exception as exc:
            return DbHealth(status="error", detail=str(exc))

    def _scalar_int(self, sql: str, params: dict[str, Any] | None = None) -> int:
        with self.engine.connect() as conn:
            value = conn.execute(text(sql), params or {}).scalar()
        return int(value or 0)

    def _scalar_decimal(self, sql: str, params: dict[str, Any] | None = None) -> float:
        with self.engine.connect() as conn:
            value = conn.execute(text(sql), params or {}).scalar()
        return round(float(value or 0), 2)

    def _rows(self, sql: str, params: dict[str, Any] | None = None) -> list[dict[str, Any]]:
        with self.engine.connect() as conn:
            rows = conn.execute(text(sql), params or {}).mappings().all()
        return [dict(row) for row in rows]

    def list_known_tables(self) -> list[str]:
        sql = """
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = 'public'
              AND table_type = 'BASE TABLE'
            ORDER BY table_name
        """
        return [row["table_name"] for row in self._rows(sql)]

    def get_money_stats(self) -> dict[str, Any]:
        """Returns donation/fundraising numbers from HelpRequests and Payments."""
        help_request_sql = """
            SELECT
                COUNT(*) AS total_requests,
                COUNT(*) FILTER (WHERE COALESCE("IsActive", false) = true) AS active_requests,
                COALESCE(SUM("TargetAmount"), 0) AS target_amount,
                COALESCE(SUM("CollectedAmount"), 0) AS collected_amount
            FROM "HelpRequests"
        """

        payment_sql = """
            SELECT
                COUNT(*) AS payments_count,
                COALESCE(SUM("Amount"), 0) AS payments_amount
            FROM "Payments"
        """

        with self.engine.connect() as conn:
            hr = conn.execute(text(help_request_sql)).mappings().one()
            payments = conn.execute(text(payment_sql)).mappings().one()

        target_amount = float(hr["target_amount"] or 0)
        collected_amount = float(hr["collected_amount"] or 0)
        completion_percent = round((collected_amount / target_amount) * 100, 1) if target_amount > 0 else 0.0

        return {
            "total_requests": int(hr["total_requests"] or 0),
            "active_requests": int(hr["active_requests"] or 0),
            "target_amount": round(target_amount, 2),
            "collected_amount": round(collected_amount, 2),
            "completion_percent": completion_percent,
            "payments_count": int(payments["payments_count"] or 0),
            "payments_amount": round(float(payments["payments_amount"] or 0), 2),
        }

    def get_help_request_stats(self) -> dict[str, Any]:
        base = self.get_money_stats()
        items_count = self._scalar_int('SELECT COUNT(*) FROM "RequestItems"')
        support_types_count = self._scalar_int('SELECT COUNT(*) FROM "SupportTypes"')
        mapped_requests = self._scalar_int('''
            SELECT COUNT(*)
            FROM "HelpRequests"
            WHERE "Latitude" IS NOT NULL AND "Longitude" IS NOT NULL
        ''')
        completed_requests = self._scalar_int('''
            SELECT COUNT(*)
            FROM "HelpRequests"
            WHERE "TargetAmount" > 0 AND "CollectedAmount" >= "TargetAmount"
        ''')
        base.update({
            "request_items_count": items_count,
            "support_types_count": support_types_count,
            "mapped_requests": mapped_requests,
            "completed_requests": completed_requests,
        })
        return base

    def get_user_stats(self) -> dict[str, Any]:
        total_users = self._scalar_int('SELECT COUNT(*) FROM "AspNetUsers"')
        profiles = self._scalar_int('SELECT COUNT(*) FROM "Profiles"')
        verified = self._scalar_int('SELECT COUNT(*) FROM "Profiles" WHERE COALESCE("IsVerified", false) = true')
        volunteers = self._scalar_int('SELECT COUNT(*) FROM "Profiles" WHERE "VerifiedAs" = 1')
        military = self._scalar_int('SELECT COUNT(*) FROM "Profiles" WHERE "VerifiedAs" = 2')
        follows = self._scalar_int('SELECT COUNT(*) FROM "Follows"')
        ratings = self._scalar_int('SELECT COUNT(*) FROM "ProfileRatings"')
        return {
            "total_users": total_users,
            "profiles": profiles,
            "verified_profiles": verified,
            "volunteers": volunteers,
            "military": military,
            "follows": follows,
            "profile_ratings": ratings,
        }

    def get_content_stats(self) -> dict[str, Any]:
        posts = self._scalar_int('''
            SELECT COUNT(*)
            FROM "Posts"
            WHERE COALESCE("PostType", 'Post') = 'Post'
        ''')
        help_requests = self._scalar_int('SELECT COUNT(*) FROM "HelpRequests"')
        comments = self._scalar_int('SELECT COUNT(*) FROM "PostComments"')
        likes = self._scalar_int('SELECT COUNT(*) FROM "PostLikes"')
        chats = self._scalar_int('SELECT COUNT(*) FROM "Chats"')
        old_pg_messages = self._scalar_int('SELECT COUNT(*) FROM "Messages"')
        return {
            "posts": posts,
            "help_requests": help_requests,
            "comments": comments,
            "likes": likes,
            "chats": chats,
            "postgres_messages": old_pg_messages,
        }

    def get_verification_stats(self) -> dict[str, Any]:
        sql = '''
            SELECT
                COUNT(*) AS total,
                COUNT(*) FILTER (WHERE "Status" = 0) AS pending,
                COUNT(*) FILTER (WHERE "Status" = 1) AS approved,
                COUNT(*) FILTER (WHERE "Status" = 2) AS rejected,
                COUNT(*) FILTER (WHERE "VerificationType" = 1) AS volunteer_requests,
                COUNT(*) FILTER (WHERE "VerificationType" = 2) AS military_requests
            FROM "VerificationRequests"
        '''
        row = self._rows(sql)[0]
        return {key: int(row[key] or 0) for key in row.keys()}

    def get_top_help_requests(self, limit: int = 5) -> list[dict[str, Any]]:
        limit = max(1, min(int(limit), 20))
        sql = f'''
            SELECT
                hr."Id"::text AS id,
                p."Content" AS content,
                u."UserName" AS username,
                hr."TargetAmount" AS target_amount,
                hr."CollectedAmount" AS collected_amount,
                CASE
                    WHEN hr."TargetAmount" > 0 THEN ROUND((hr."CollectedAmount" / hr."TargetAmount") * 100, 1)
                    ELSE 0
                END AS completion_percent,
                hr."IsActive" AS is_active,
                p."CreatedAt" AS created_at
            FROM "HelpRequests" hr
            INNER JOIN "Posts" p ON p."Id" = hr."Id"
            LEFT JOIN "AspNetUsers" u ON u."Id" = p."UserId"
            ORDER BY hr."CollectedAmount" DESC, p."CreatedAt" DESC
            LIMIT {limit}
        '''
        rows = self._rows(sql)
        return [self._format_help_request_row(row) for row in rows]

    def get_recent_help_requests(self, limit: int = 5) -> list[dict[str, Any]]:
        limit = max(1, min(int(limit), 20))
        sql = f'''
            SELECT
                hr."Id"::text AS id,
                p."Content" AS content,
                u."UserName" AS username,
                hr."TargetAmount" AS target_amount,
                hr."CollectedAmount" AS collected_amount,
                CASE
                    WHEN hr."TargetAmount" > 0 THEN ROUND((hr."CollectedAmount" / hr."TargetAmount") * 100, 1)
                    ELSE 0
                END AS completion_percent,
                hr."IsActive" AS is_active,
                p."CreatedAt" AS created_at
            FROM "HelpRequests" hr
            INNER JOIN "Posts" p ON p."Id" = hr."Id"
            LEFT JOIN "AspNetUsers" u ON u."Id" = p."UserId"
            ORDER BY p."CreatedAt" DESC
            LIMIT {limit}
        '''
        rows = self._rows(sql)
        return [self._format_help_request_row(row) for row in rows]

    def get_support_type_distribution(self) -> list[dict[str, Any]]:
        sql = '''
            SELECT
                COALESCE(st."NameOfType", 'Без типу') AS support_type,
                COUNT(ri."Id") AS items_count,
                COALESCE(SUM(ri."Quantity"), 0) AS total_quantity,
                COALESCE(SUM(ri."Quantity" * ri."UnitPrice"), 0) AS estimated_amount
            FROM "RequestItems" ri
            LEFT JOIN "SupportTypes" st ON st."Id" = ri."SupportTypeId"
            GROUP BY COALESCE(st."NameOfType", 'Без типу')
            ORDER BY items_count DESC, estimated_amount DESC
        '''
        rows = self._rows(sql)
        return [
            {
                "support_type": row["support_type"],
                "items_count": int(row["items_count"] or 0),
                "total_quantity": int(row["total_quantity"] or 0),
                "estimated_amount": round(float(row["estimated_amount"] or 0), 2),
            }
            for row in rows
        ]

    def get_dashboard_kpi(self) -> dict[str, Any]:
        return {
            "money": self.get_money_stats(),
            "help_requests": self.get_help_request_stats(),
            "users": self.get_user_stats(),
            "content": self.get_content_stats(),
            "verification": self.get_verification_stats(),
            "top_help_requests": self.get_top_help_requests(5),
            "support_types": self.get_support_type_distribution(),
        }

    def ensure_faq_log_table(self) -> None:
        sql = text('''
            CREATE TABLE IF NOT EXISTS "FaqQuestionLogs" (
                "Id" BIGSERIAL PRIMARY KEY,
                "Question" TEXT NOT NULL,
                "Intent" VARCHAR(120) NULL,
                "Domain" VARCHAR(120) NULL,
                "Source" VARCHAR(80) NULL,
                "Confidence" DOUBLE PRECISION NULL,
                "Answer" TEXT NULL,
                "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
            )
        ''')
        with self.engine.begin() as conn:
            conn.execute(sql)

    def log_faq_question(
        self,
        question: str,
        answer: str,
        intent: str | None = None,
        domain: str | None = None,
        source: str | None = None,
        confidence: float | None = None,
    ) -> None:
        if not question or not question.strip():
            return
        try:
            self.ensure_faq_log_table()
            sql = text('''
                INSERT INTO "FaqQuestionLogs"
                    ("Question", "Intent", "Domain", "Source", "Confidence", "Answer")
                VALUES
                    (:question, :intent, :domain, :source, :confidence, :answer)
            ''')
            with self.engine.begin() as conn:
                conn.execute(sql, {
                    "question": question.strip(),
                    "intent": intent,
                    "domain": domain,
                    "source": source,
                    "confidence": confidence,
                    "answer": answer,
                })
        except Exception:
            return

    def get_recent_faq_questions(self, limit: int = 20) -> list[dict[str, Any]]:
        limit = max(1, min(int(limit), 100))
        try:
            self.ensure_faq_log_table()
            sql = text(f'''
                SELECT "Id", "Question", "Intent", "Domain", "Source", "Confidence", "Answer", "CreatedAt"
                FROM "FaqQuestionLogs"
                ORDER BY "CreatedAt" DESC
                LIMIT {limit}
            ''')
            rows = self._rows(str(sql))
            return [
                {
                    "id": row["Id"],
                    "question": row["Question"],
                    "intent": row["Intent"],
                    "domain": row["Domain"],
                    "source": row["Source"],
                    "confidence": row["Confidence"],
                    "answer": row["Answer"],
                    "created_at": self._safe_datetime_to_str(row["CreatedAt"]),
                }
                for row in rows
            ]
        except Exception:
            return []

    @staticmethod
    def _format_help_request_row(row: dict[str, Any]) -> dict[str, Any]:
        content = (row.get("content") or "").strip()
        if len(content) > 180:
            content = content[:177] + "..."
        return {
            "id": row.get("id"),
            "content": content,
            "username": row.get("username") or "невідомо",
            "target_amount": round(float(row.get("target_amount") or 0), 2),
            "collected_amount": round(float(row.get("collected_amount") or 0), 2),
            "completion_percent": round(float(row.get("completion_percent") or 0), 1),
            "is_active": bool(row.get("is_active")),
            "created_at": SupportWayDataTools._safe_datetime_to_str(row.get("created_at")),
        }
