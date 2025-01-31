import { IUser } from "../auth/user";

export interface IComment {
  commentId:number,
  userId:number,
  parentId:number | null,
  content:string,
  creationDate:Date,
  lastEdit:Date,
  isActive:boolean,
  creator:IUser | null
}
