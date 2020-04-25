import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BottomSheetTemplateComponent } from './bottom-sheet-template.component';

describe('BottomSheetTemplateComponent', () => {
  let component: BottomSheetTemplateComponent;
  let fixture: ComponentFixture<BottomSheetTemplateComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BottomSheetTemplateComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BottomSheetTemplateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
