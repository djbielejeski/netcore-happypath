import { Component, OnInit } from '@angular/core';

import { UserService } from '@app/core/services';

import { UserModel, NewUserModel } from '@app/shared/models';

@Component({
  selector: 'netcore-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss']
})
export class UsersComponent implements OnInit {
    private users: UserModel[] = [];
    private newUser: NewUserModel = {
        FirstName: "David",
        LastName: "Bielejeski",
        EmailAddress: "davidsEmailAddress@gmail.com"
    };

    constructor(private userService: UserService) {

    }

    ngOnInit() {
        this.getUsers();
    }

    getUsers() {
        this.userService.getUsers().subscribe((users: UserModel[]) => {
            this.users = users;
        });
    }

    addUser() {
        this.userService.addUser(this.newUser).subscribe(() => {
            this.getUsers();
        });
    }
}
