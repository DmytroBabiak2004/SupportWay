import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Message } from '../../../services/chat.service';

@Component({
  selector: 'app-message-bubble',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './message-bubble.component.html',
  styleUrls: ['./message-bubble.component.scss']
})
export class MessageBubbleComponent {
  @Input() msg!: Message;
  @Input() isMine: boolean = false;

  @Output() deleteRequest = new EventEmitter<string>();

  onDelete() {
    this.deleteRequest.emit(this.msg.id);
  }
}
