import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-root',
  // templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  template: `
  <div role="main" [ngClass]="{'my-theme': defaultTheme, 'my-dark-theme': !defaultTheme}">
    <div class='row'>
      <div>
        <app-nav-bar (change)="this.defaultTheme=$event"></app-nav-bar>
      </div>
    </div>
  </div>
  `
})
export class AppComponent {
  defaultTheme = true;

  // tslint:disable-next-line: use-lifecycle-interface
  ngOnInit() { }
}
