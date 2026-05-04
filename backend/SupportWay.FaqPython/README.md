# SupportWay Python FAQ Assistant

Python/FastAPI microservice for the SupportWay FAQ bot. ASP.NET Core calls this service through `FaqBotService`.

## Run

```bash
cd backend/SupportWay.FaqPython
python -m venv .venv
.venv\Scripts\activate
pip install -r requirements.txt
uvicorn main:app --host 127.0.0.1 --port 8010 --reload
```

## Optional Gemini

Create `.env` from `.env.example` and set `GEMINI_API_KEY`. Without it, the bot still works in deterministic keyword mode.

## Test

```bash
curl -X POST http://127.0.0.1:8010/faq -H "Content-Type: application/json" -d "{\"question\":\"як пройти верифікацію\",\"roles\":[\"User\"]}"
```
