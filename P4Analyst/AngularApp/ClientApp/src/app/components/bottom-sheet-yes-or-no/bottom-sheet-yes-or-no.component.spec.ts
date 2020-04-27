import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BottomSheetYesOrNoComponent } from './bottom-sheet-yes-or-no.component';

describe('BottomSheetYesOrNoComponent', () => {
  let component: BottomSheetYesOrNoComponent;
  let fixture: ComponentFixture<BottomSheetYesOrNoComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BottomSheetYesOrNoComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BottomSheetYesOrNoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
