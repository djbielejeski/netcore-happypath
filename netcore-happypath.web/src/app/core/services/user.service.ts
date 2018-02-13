import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';

import { UserModel, NewUserModel } from '@app/shared/models';

@Injectable()
export class UserService {
    constructor(private http: HttpClient) { }

    getUsers(): Observable<UserModel[]> {
        return this.http.get<UserModel[]>('/api/users');
    }

    addUser(user: NewUserModel): Observable<void> {
        return this.http.post<void>('/api/users', user);
    }
}
