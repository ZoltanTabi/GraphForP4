import { MediaMatcher } from '@angular/cdk/layout';
import { ChangeDetectorRef, Component, OnDestroy } from '@angular/core';
import { LocalStorageService } from 'ngx-store';
import { ThemeService } from 'ng2-charts';
import { ChartOptions } from 'chart.js';

@Component({
  selector: 'app-nav-bar',
  templateUrl: './nav-bar.component.html',
  styleUrls: ['./nav-bar.component.scss']
})
export class NavBarComponent implements OnDestroy {
  mobileQuery: MediaQueryList;
  opened = true;
  toggleChecked = false;

  private _mobileQueryListener: () => void;

  // tslint:disable-next-line:max-line-length
  constructor(changeDetectorRef: ChangeDetectorRef, media: MediaMatcher, private localStorageService: LocalStorageService, private themeService: ThemeService) {
    const checked = localStorageService.get('darkTheme') as boolean;
    if (checked) {
      this.changeToggle(true);
    }
    this.mobileQuery = media.matchMedia('(max-width: 600px)');
    this._mobileQueryListener = () => changeDetectorRef.detectChanges();
    // tslint:disable-next-line: deprecation
    this.mobileQuery.addListener(this._mobileQueryListener);
  }

  ngOnDestroy(): void {
    // tslint:disable-next-line: deprecation
    this.mobileQuery.removeListener(this._mobileQueryListener);
  }

  changeToggle(checked: boolean) {
    this.localStorageService.set('darkTheme', checked);
    this.toggleChecked = checked;
    let overrides: ChartOptions;
    const index = document.getElementById('index');
    if (checked) {
      index.classList.remove('theme');
      index.classList.add('dark-theme');
      overrides = {
        legend: {
          labels: { fontColor: 'white' }
        },
        scales: {
          xAxes: [{
            ticks: { fontColor: 'white' },
            gridLines: { color: 'rgba(255,255,255,0.1)' }
          }],
          yAxes: [{
            ticks: { fontColor: 'white' },
            gridLines: { color: 'rgba(255,255,255,0.1)' }
          }]
        }
      };
    } else {
      index.classList.remove('dark-theme');
      index.classList.add('theme');
      overrides = {};
    }
    this.themeService.setColorschemesOptions(overrides);
  }
}
