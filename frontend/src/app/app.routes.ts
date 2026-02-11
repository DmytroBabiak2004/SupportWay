import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';

import { AuthorizationComponent } from './components/authorization/authorization.component';
import { HomeComponent } from './components/home/home.component';
import { ChatPageComponent } from './components/chat/chat-page/chat-page.component'
import {UserProfileComponent} from './components/user-profile/user-profile.component';
import {PostsComponent} from './components/posts/posts.component';

export const routes: Routes = [
  { path: 'login', component: AuthorizationComponent },
  { path: 'home', component: HomeComponent, canActivate: [AuthGuard] },
  { path: 'chat', component: ChatPageComponent, canActivate: [AuthGuard] },
  { path: 'posts', component: PostsComponent, canActivate: [AuthGuard] },
  { path: 'profile/:userId', component: UserProfileComponent, canActivate: [AuthGuard] },

  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: '**', redirectTo: 'home' }
];
