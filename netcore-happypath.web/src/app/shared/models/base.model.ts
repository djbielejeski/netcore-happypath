export interface BaseModel<T> {
    Id: T; 
    CreateUser: string;
    CreateDateTime: Date;
    UpdateUser: string;
    UpdateDateTime: Date;
    CreateDateTimeDisplay: string;
}
