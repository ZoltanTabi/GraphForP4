import { Component, Inject } from '@angular/core';
import { MatSnackBarRef, MAT_SNACK_BAR_DATA } from '@angular/material';
import { Data } from '../../models/data';

@Component({
  selector: 'app-snack-bar-template',
  templateUrl: './snack-bar-template.component.html',
  styleUrls: ['./snack-bar-template.component.scss']
})
export class SnackBarTemplateComponent {
  constructor(
    public snackBarRef: MatSnackBarRef<SnackBarTemplateComponent>,
    @Inject(MAT_SNACK_BAR_DATA) public data: Data) { }
}
