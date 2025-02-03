import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar'

@Injectable({
  providedIn: 'root'
})
export class UtilitiesService {

  constructor(private snackBar:MatSnackBar) { }

  Alert(message:string, type:string){
    this.snackBar.open(message, type, {
      horizontalPosition:"center",
      verticalPosition:"top",
      duration:3000
    });
  }

}
