import { Component, Inject } from '@angular/core';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material';
import { Data } from '../../models/data';

@Component({
  selector: 'app-bottom-sheet-template',
  templateUrl: './bottom-sheet-template.component.html',
  styleUrls: ['./bottom-sheet-template.component.scss']
})
export class BottomSheetTemplateComponent {

  constructor(
    public bottomSheetRef: MatBottomSheetRef<BottomSheetTemplateComponent>,
    @Inject(MAT_BOTTOM_SHEET_DATA) public data: Data[]) { }

    action(id: number): void {
      this.bottomSheetRef.dismiss(id);
    }
}
