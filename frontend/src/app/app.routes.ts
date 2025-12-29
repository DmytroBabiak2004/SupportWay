import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';

import { AuthorizationComponent } from './components/authorization/authorization.component';
import { HomeComponent } from './components/home/home.component';
import { ChatPageComponent } from  './components/chat-page/chat-page.component'

export const routes: Routes = [
  { path: 'login', component: AuthorizationComponent },
  { path: 'home', component: HomeComponent, canActivate: [AuthGuard] },
  { path: 'chat', component: ChatPageComponent, canActivate: [AuthGuard] },


  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: '**', redirectTo: 'home' }
];
