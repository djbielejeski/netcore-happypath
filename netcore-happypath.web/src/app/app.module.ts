import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';


import { routes } from '@app/routes';
import { AppRootComponent } from '@app/app-root/app-root.component';
import { UsersComponent } from '@app/users/users.component';

import * as services from '@app/core/services';

@NgModule({
    declarations: [
        AppRootComponent,
        UsersComponent
    ],
    imports: [
        BrowserModule,
        FormsModule,
        RouterModule.forRoot(routes), //, { enableTracing: true }
        HttpClientModule,
    ],
    providers: [
        services.UserService
    ],
    bootstrap: [AppRootComponent]
})
export class AppModule { }
