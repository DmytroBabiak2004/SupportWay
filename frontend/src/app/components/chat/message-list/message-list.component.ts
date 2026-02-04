import {
  AfterViewChecked,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  Input,
  OnChanges,
  SimpleChanges,
  ViewChild
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Message } from '../../../services/chat.service';
import { MessageBubbleComponent } from '../message-bubble/message-bubble.component';

@Component({
  selector: 'app-message-list',
  standalone: true,
  imports: [CommonModule, MessageBubbleComponent],
  templateUrl: './message-list.component.html',
  styleUrls: ['./message-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MessageListComponent implements OnChanges, AfterViewChecked {
  @Input() messages: Message[] = [];
  @Input() currentUserId = '';

  @ViewChild('scrollBox') private scrollBox?: ElementRef<HTMLElement>;

  private shouldScroll = false;
  private prevLen = 0;

  ngOnChanges(changes: SimpleChanges): void {
    if ('messages' in changes) {
      const nextLen = this.messages?.length ?? 0;
      if (nextLen !== this.prevLen) {
        this.prevLen = nextLen;
        this.shouldScroll = true;
      }
    }
  }

  ngAfterViewChecked(): void {
    if (!this.shouldScroll) return;
    this.shouldScroll = false;
    const el = this.scrollBox?.nativeElement;
    if (!el) return;
    el.scrollTop = el.scrollHeight;
  }

  trackByMsgId = (_: number, msg: Message) => (msg as any).id ?? msg.sentAt ?? _;
}
