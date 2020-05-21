import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GraphCarouselComponent } from './graph-carousel.component';

describe('GraphCarouselComponent', () => {
  let component: GraphCarouselComponent;
  let fixture: ComponentFixture<GraphCarouselComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GraphCarouselComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GraphCarouselComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
