import {
  AfterViewInit,
  Component,
  ElementRef,
  EventEmitter,
  Input,
  Output,
  SimpleChanges,
  ViewChild,
  OnChanges
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MessageBubbleComponent } from '../message-bubble/message-bubble.component';
import { Message } from '../../../services/chat.service';

@Component({
  selector: 'app-message-list',
  standalone: true,
  imports: [CommonModule, MessageBubbleComponent],
  templateUrl: './message-list.component.html',
  styleUrls: ['./message-list.component.scss']
})
export class MessageListComponent implements AfterViewInit, OnChanges {
  @Input() messages: Message[] = [];
  @Input() currentUserId!: string;
  @Output() deleteRequest = new EventEmitter<string>();

  @ViewChild('scrollBox') scrollBox!: ElementRef<HTMLElement>;

  private initialScrollDone = false;

  trackByMsgId(_: number, msg: Message): string {
    return msg.id;
  }

  onDelete(messageId: string): void {
    this.deleteRequest.emit(messageId);
  }

  ngAfterViewInit(): void {
    this.scrollToBottom(true);
    this.initialScrollDone = true;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['messages']) {
      queueMicrotask(() => {
        if (!this.scrollBox) return;

        if (!this.initialScrollDone) {
          this.scrollToBottom(true);
          return;
        }

        this.scrollToBottom(true);
      });
    }
  }

  shouldShowDateSeparator(index: number): boolean {
    if (index === 0) return true;

    const current = this.messages[index];
    const previous = this.messages[index - 1];

    if (!current?.sentAt || !previous?.sentAt) return false;

    return !this.isSameDay(current.sentAt, previous.sentAt);
  }

  formatSeparatorDate(value: string | Date): string {
    const date = new Date(value);
    const now = new Date();

    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const target = new Date(date.getFullYear(), date.getMonth(), date.getDate());

    const diffMs = today.getTime() - target.getTime();
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffDays === 0) return 'Сьогодні';
    if (diffDays === 1) return 'Вчора';

    const sameYear = now.getFullYear() === date.getFullYear();

    return new Intl.DateTimeFormat('uk-UA', {
      day: 'numeric',
      month: 'long',
      ...(sameYear ? {} : { year: 'numeric' })
    }).format(date);
  }

  private isSameDay(a: string | Date, b: string | Date): boolean {
    const dateA = new Date(a);
    const dateB = new Date(b);

    return (
      dateA.getFullYear() === dateB.getFullYear() &&
      dateA.getMonth() === dateB.getMonth() &&
      dateA.getDate() === dateB.getDate()
    );
  }

  private scrollToBottom(smooth = false): void {
    if (!this.scrollBox) return;

    const el = this.scrollBox.nativeElement;
    el.scrollTo({
      top: el.scrollHeight,
      behavior: smooth ? 'smooth' : 'auto'
    });
  }
}
