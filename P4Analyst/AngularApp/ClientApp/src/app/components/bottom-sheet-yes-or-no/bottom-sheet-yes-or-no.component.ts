import { Component, Inject } from '@angular/core';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material';
import { Data } from '../../models/data';

@Component({
  selector: 'app-bottom-sheet-yes-or-no',
  templateUrl: './bottom-sheet-yes-or-no.component.html',
  styleUrls: ['./bottom-sheet-yes-or-no.component.scss']
})
export class BottomSheetYesOrNoComponent {

  constructor(
  public bottomSheetRef: MatBottomSheetRef<BottomSheetYesOrNoComponent>,
  @Inject(MAT_BOTTOM_SHEET_DATA) public data: Data) { }

  answer(answer: boolean): void {
    this.bottomSheetRef.dismiss(answer);
  }

}
