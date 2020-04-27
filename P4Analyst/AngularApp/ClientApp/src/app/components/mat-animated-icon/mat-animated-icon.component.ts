import { Component, Input } from '@angular/core';

@Component({
  // tslint:disable-next-line: component-selector
  selector: 'mat-animated-icon',
  templateUrl: './mat-animated-icon.component.html',
  styleUrls: ['./mat-animated-icon.component.scss']
})
export class MatAnimatedIconComponent {

  @Input() start: String;
  @Input() end: String;
  @Input() colorStart: String;
  @Input() colorEnd: String;
  @Input() animate: boolean;
  @Input() animateFromParent = false;
  @Input() playAndPause = false;

  public toggle() {
    if (!this.animateFromParent) {
      this.animate = !this.animate;
    }
  }
}
