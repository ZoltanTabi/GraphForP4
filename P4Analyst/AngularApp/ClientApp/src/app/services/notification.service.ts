import { Injectable, NgZone } from '@angular/core';
import {
  MatSnackBar,
  MatSnackBarConfig,
  MatSnackBarVerticalPosition,
  MatSnackBarHorizontalPosition
} from '@angular/material';
import { SnackBarTemplateComponent } from '../components/snack-bar-template/snack-bar-template.component';
import { Data } from '../models/data';

@Injectable()
export class NotificationService {
  constructor(
    private readonly snackBar: MatSnackBar,
    private readonly zone: NgZone
  ) { }

  info(message: string, horizontalPosition: MatSnackBarHorizontalPosition = 'center',
       verticalPosition: MatSnackBarVerticalPosition = 'bottom') {
    const data: Data = {icon: 'info', text: message};
    this.open({
      data: data,
      panelClass: 'info-notification',
      horizontalPosition: horizontalPosition,
      verticalPosition: verticalPosition,
      duration: 3500
    });
  }

  success(message: string, horizontalPosition: MatSnackBarHorizontalPosition = 'center',
          verticalPosition: MatSnackBarVerticalPosition = 'bottom') {
    const data: Data = {icon: 'check_circle', text: message};
    this.open({
      data: data,
      panelClass: 'success-notification',
      horizontalPosition: horizontalPosition,
      verticalPosition: verticalPosition,
      duration: 3500
    });
  }

  warning(message: string, horizontalPosition: MatSnackBarHorizontalPosition = 'center',
       verticalPosition: MatSnackBarVerticalPosition = 'bottom') {
    const data: Data = {icon: 'warning', text: message};
    this.open({
      data: data,
      panelClass: 'warning-notification',
      horizontalPosition: horizontalPosition,
      verticalPosition: verticalPosition
    });
  }

  error(message: string, horizontalPosition: MatSnackBarHorizontalPosition = 'center',
        verticalPosition: MatSnackBarVerticalPosition = 'bottom') {
    const data: Data = {icon: 'error', text: message};
    this.open({
      data: data,
      panelClass: 'error-notification',
      horizontalPosition: horizontalPosition,
      verticalPosition: verticalPosition
    });
  }

  private open(configuration: MatSnackBarConfig) {
    if (!configuration.duration) {
      configuration.duration = 5000;
    }

    this.zone.run(() => this.snackBar.openFromComponent(SnackBarTemplateComponent, configuration));
  }
}
