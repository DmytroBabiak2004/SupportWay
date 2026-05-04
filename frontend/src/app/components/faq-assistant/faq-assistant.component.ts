import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs/operators';

import { FaqBotService } from '../../services/faq-bot.service';
import { FaqChatMessage, FaqSuggestion } from '../../models/faq-bot.model';

@Component({
  selector: 'app-faq-assistant',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './faq-assistant.component.html',
  styleUrls: ['./faq-assistant.component.scss']
})
export class FaqAssistantComponent implements OnInit {
  private faqBotService = inject(FaqBotService);
  private router = inject(Router);

  @ViewChild('messagesPanel') messagesPanel?: ElementRef<HTMLDivElement>;

  isOpen = false;
  isLoading = false;
  question = '';
  suggestions: FaqSuggestion[] = [];

  messages: FaqChatMessage[] = [
    {
      id: 'welcome',
      sender: 'bot',
      category: 'SupportWay Assistant',
      text: 'Привіт! Я допоможу розібратися з SupportWay: верифікацією, запитами допомоги, донатами, картою, публікаціями та чатами.',
      suggestions: [
        'Як пройти верифікацію?',
        'Як створити запит допомоги?',
        'Як працюють донати?',
        'Як поділитися постом у чат?'
      ]
    }
  ];

  ngOnInit(): void {
    this.loadSuggestions();
  }

  toggleAssistant(): void {
    this.isOpen = !this.isOpen;

    if (this.isOpen) {
      setTimeout(() => this.scrollToBottom(), 80);
    }
  }

  closeAssistant(): void {
    this.isOpen = false;
  }

  askFromSuggestion(text: string): void {
    this.question = text;
    this.sendQuestion();
  }

  sendQuestion(): void {
    const value = this.question.trim();

    if (!value || this.isLoading) {
      return;
    }

    this.messages.push({
      id: this.createId(),
      sender: 'user',
      text: value
    });

    const loadingId = this.createId();
    this.messages.push({
      id: loadingId,
      sender: 'bot',
      text: 'Шукаю відповідь...',
      isLoading: true
    });

    this.question = '';
    this.isLoading = true;
    setTimeout(() => this.scrollToBottom(), 50);

    this.faqBotService.ask(value)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: response => {
          this.removeMessage(loadingId);
          this.messages.push({
            id: this.createId(),
            sender: 'bot',
            text: response.answer,
            category: response.category,
            actions: response.actions ?? [],
            suggestions: response.suggestions ?? []
          });
          setTimeout(() => this.scrollToBottom(), 50);
        },
        error: () => {
          this.removeMessage(loadingId);
          this.messages.push({
            id: this.createId(),
            sender: 'bot',
            category: 'Помилка',
            text: 'Не вдалося отримати відповідь від помічника. Перевірте, чи запущений backend, і спробуйте ще раз.',
            suggestions: ['Як пройти верифікацію?', 'Як створити запит допомоги?']
          });
          setTimeout(() => this.scrollToBottom(), 50);
        }
      });
  }

  goTo(route: string): void {
    if (!route) {
      return;
    }

    this.closeAssistant();
    this.router.navigate([route]);
  }

  trackByMessageId(_: number, message: FaqChatMessage): string {
    return message.id;
  }

  trackBySuggestion(_: number, suggestion: FaqSuggestion): string {
    return `${suggestion.category}-${suggestion.text}`;
  }

  private loadSuggestions(): void {
    this.faqBotService.getSuggestions().subscribe({
      next: suggestions => this.suggestions = suggestions ?? [],
      error: () => {
        this.suggestions = [
          { text: 'Як пройти верифікацію?', category: 'Верифікація' },
          { text: 'Як створити запит допомоги?', category: 'Запити' },
          { text: 'Як працюють донати?', category: 'Донати' },
          { text: 'Як працює карта?', category: 'Карта' }
        ];
      }
    });
  }

  private removeMessage(id: string): void {
    this.messages = this.messages.filter(message => message.id !== id);
  }

  private scrollToBottom(): void {
    const element = this.messagesPanel?.nativeElement;
    if (!element) {
      return;
    }

    element.scrollTo({ top: element.scrollHeight, behavior: 'smooth' });
  }

  private createId(): string {
    return `${Date.now()}-${Math.random().toString(16).slice(2)}`;
  }
}
