import { Routes } from '@angular/router';

import { UsersComponent } from '@app/users/users.component';

export const routes: Routes = [
  { path: 'users', component: UsersComponent },
  // Wildcard route
  { path: '**', redirectTo: '/users', pathMatch: 'full' }
];
