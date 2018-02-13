import { BaseModel } from './base.model';
export interface UserModel extends BaseModel<string> {
    FirstName: string;
    LastName: string;
    EmailAddress: string;
}

export interface NewUserModel {
    FirstName: string;
    LastName: string;
    EmailAddress: string;
}
