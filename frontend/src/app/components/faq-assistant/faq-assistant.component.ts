import {
  AfterViewChecked,
  Component,
  ElementRef,
  HostListener,
  OnInit,
  ViewChild,
  inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { FaqBotService } from '../../services/faq-bot.service';

interface AssistantAction {
  label: string;
  route: string;
}

interface AssistantMessage {
  id: string;
  sender: 'user' | 'bot';
  text: string;
  category?: string | null;
  isLoading?: boolean;
  actions?: AssistantAction[];
  suggestions?: string[];
}

interface AssistantSuggestion {
  text: string;
}

@Component({
  selector: 'app-faq-assistant',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './faq-assistant.component.html',
  styleUrls: ['./faq-assistant.component.scss']
})
export class FaqAssistantComponent implements OnInit, AfterViewChecked {
  private faqBotService = inject(FaqBotService);
  private router = inject(Router);

  @ViewChild('messagesPanel') messagesPanel?: ElementRef<HTMLDivElement>;

  isOpen = false;
  isLoading = false;
  question = '';

  messages: AssistantMessage[] = [];
  suggestions: AssistantSuggestion[] = [];

  position = {
    x: window.innerWidth - 96,
    y: window.innerHeight - 96
  };

  horizontalDock: 'left' | 'right' = 'right';
  verticalDock: 'top' | 'bottom' = 'bottom';

  private readonly launcherSize = 64;
  private readonly edgePadding = 18;
  private readonly dragThreshold = 6;

  private isDragging = false;
  private didDrag = false;
  private activePointerId: number | null = null;
  private dragOffsetX = 0;
  private dragOffsetY = 0;
  private pendingScroll = false;

  ngOnInit(): void {
    this.restorePosition();
    this.updateDock();
    this.clampPosition();

    this.loadSuggestions();

    if (this.messages.length === 0) {
      this.messages.push({
        id: this.createId(),
        sender: 'bot',
        text: 'Привіт! Я SupportWay Assistant. Я можу допомогти з питаннями про верифікацію, запити, пости, карту, донати та чат.',
        category: 'Помічник'
      });
    }
  }

  ngAfterViewChecked(): void {
    if (this.pendingScroll) {
      this.scrollToBottom();
      this.pendingScroll = false;
    }
  }

  toggleAssistant(): void {
    this.isOpen = !this.isOpen;

    if (this.isOpen) {
      this.pendingScroll = true;
    }
  }

  closeAssistant(): void {
    this.isOpen = false;
  }

  onLauncherClick(event: MouseEvent): void {
    if (this.didDrag) {
      event.preventDefault();
      event.stopPropagation();
      this.didDrag = false;
      return;
    }

    this.toggleAssistant();
  }

  onLauncherPointerDown(event: PointerEvent): void {
    if (event.button !== 0 && event.pointerType !== 'touch') {
      return;
    }

    this.activePointerId = event.pointerId;
    this.isDragging = true;
    this.didDrag = false;

    this.dragOffsetX = event.clientX - this.position.x;
    this.dragOffsetY = event.clientY - this.position.y;

    const target = event.currentTarget as HTMLElement | null;
    target?.setPointerCapture?.(event.pointerId);

    event.preventDefault();
  }

  @HostListener('document:pointermove', ['$event'])
  onPointerMove(event: PointerEvent): void {
    if (!this.isDragging || this.activePointerId !== event.pointerId) {
      return;
    }

    const nextX = event.clientX - this.dragOffsetX;
    const nextY = event.clientY - this.dragOffsetY;

    if (
      Math.abs(nextX - this.position.x) > this.dragThreshold ||
      Math.abs(nextY - this.position.y) > this.dragThreshold
    ) {
      this.didDrag = true;
    }

    this.position.x = nextX;
    this.position.y = nextY;

    this.clampPosition();
    this.updateDock();
    this.persistPosition();
  }

  @HostListener('document:pointerup', ['$event'])
  onPointerUp(event: PointerEvent): void {
    if (this.activePointerId !== event.pointerId) {
      return;
    }

    this.finishDrag();
  }

  @HostListener('document:pointercancel', ['$event'])
  onPointerCancel(event: PointerEvent): void {
    if (this.activePointerId !== event.pointerId) {
      return;
    }

    this.finishDrag();
  }

  @HostListener('window:resize')
  onResize(): void {
    this.clampPosition();
    this.updateDock();
    this.persistPosition();
  }

  sendQuestion(): void {
    const text = this.question.trim();
    if (!text || this.isLoading) return;

    this.messages.push({
      id: this.createId(),
      sender: 'user',
      text
    });

    const loadingMessage: AssistantMessage = {
      id: this.createId(),
      sender: 'bot',
      text: 'Готую відповідь...',
      isLoading: true
    };

    this.messages.push(loadingMessage);
    this.question = '';
    this.isLoading = true;
    this.pendingScroll = true;

    this.faqBotService.ask(text).subscribe({
      next: response => {
        this.removeLoadingMessage(loadingMessage.id);

        this.messages.push({
          id: this.createId(),
          sender: 'bot',
          text: response.answer,
          category: response.category || null,
          actions: response.actions || [],
          suggestions: response.suggestions || []
        });

        this.isLoading = false;
        this.pendingScroll = true;
      },
      error: () => {
        this.removeLoadingMessage(loadingMessage.id);

        this.messages.push({
          id: this.createId(),
          sender: 'bot',
          text: 'Не вдалося отримати відповідь. Спробуйте ще раз трохи пізніше.',
          category: 'Помилка'
        });

        this.isLoading = false;
        this.pendingScroll = true;
      }
    });
  }

  askFromSuggestion(text: string): void {
    this.question = text;
    this.sendQuestion();
  }

  goTo(route: string): void {
    if (!route) return;
    this.router.navigate([route]);
    this.closeAssistant();
  }

  trackBySuggestion(index: number, item: AssistantSuggestion): string {
    return `${index}-${item.text}`;
  }

  trackByMessageId(_: number, item: AssistantMessage): string {
    return item.id;
  }

  private loadSuggestions(): void {
    this.faqBotService.getSuggestions().subscribe({
      next: data => {
        this.suggestions = (data || []).map((item: any) =>
          typeof item === 'string' ? { text: item } : item
        );
      },
      error: () => {
        this.suggestions = [];
      }
    });
  }

  private removeLoadingMessage(id: string): void {
    this.messages = this.messages.filter(m => m.id !== id);
  }

  private finishDrag(): void {
    this.isDragging = false;
    this.activePointerId = null;

    this.clampPosition();
    this.updateDock();
    this.persistPosition();

    setTimeout(() => {
      this.didDrag = false;
    }, 0);
  }

  private clampPosition(): void {
    const maxX = Math.max(this.edgePadding, window.innerWidth - this.launcherSize - this.edgePadding);
    const maxY = Math.max(this.edgePadding, window.innerHeight - this.launcherSize - this.edgePadding);

    this.position.x = Math.min(Math.max(this.position.x, this.edgePadding), maxX);
    this.position.y = Math.min(Math.max(this.position.y, this.edgePadding), maxY);
  }

  private updateDock(): void {
    const centerX = this.position.x + this.launcherSize / 2;
    const centerY = this.position.y + this.launcherSize / 2;

    this.horizontalDock = centerX >= window.innerWidth / 2 ? 'right' : 'left';
    this.verticalDock = centerY >= window.innerHeight / 2 ? 'bottom' : 'top';
  }

  private persistPosition(): void {
    localStorage.setItem('faq-assistant-position', JSON.stringify(this.position));
  }

  private restorePosition(): void {
    const raw = localStorage.getItem('faq-assistant-position');
    if (!raw) return;

    try {
      const parsed = JSON.parse(raw);
      if (typeof parsed?.x === 'number' && typeof parsed?.y === 'number') {
        this.position = parsed;
      }
    } catch {
      // ignore
    }
  }

  private scrollToBottom(): void {
    const container = this.messagesPanel?.nativeElement;
    if (!container) return;

    container.scrollTop = container.scrollHeight;
  }

  private createId(): string {
    return `${Date.now()}-${Math.random().toString(36).slice(2, 9)}`;
  }
}
