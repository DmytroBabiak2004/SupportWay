import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {HeaderComponent} from './components/header/header.component';
import { FaqAssistantComponent } from './components/faq-assistant/faq-assistant.component';
import {ToastContainerComponent} from './components/toast/toast-container/toast-container.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent, FaqAssistantComponent, ToastContainerComponent],
  templateUrl: './app.component.html',
  standalone: true,
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'support-way-frontend';
}
