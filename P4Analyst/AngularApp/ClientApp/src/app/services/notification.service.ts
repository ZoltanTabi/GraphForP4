import { Injectable, NgZone } from '@angular/core';
import {
  MatSnackBar,
  MatSnackBarConfig,
  MatSnackBarVerticalPosition,
  MatSnackBarHorizontalPosition
} from '@angular/material';
import { SnackBarTemplateComponent } from '../components/snack-bar-template/snack-bar-template.component';

@Injectable()
export class NotificationService {
  constructor(
    private readonly snackBar: MatSnackBar,
    private readonly zone: NgZone
  ) { }

  info(message: string, horizontalPosition: MatSnackBarHorizontalPosition = 'center',
       verticalPosition: MatSnackBarVerticalPosition = 'bottom') {
    this.open({
      data: {
        message: message,
        icon: 'info'
      },
      panelClass: 'info-notification',
      horizontalPosition: horizontalPosition,
      verticalPosition: verticalPosition
    });
  }

  success(message: string, horizontalPosition: MatSnackBarHorizontalPosition = 'center',
          verticalPosition: MatSnackBarVerticalPosition = 'bottom') {
    this.open({
      data: {
        message: message,
        icon: 'ok'
      },
      panelClass: 'success-notification',
      horizontalPosition: horizontalPosition,
      verticalPosition: verticalPosition
    });
  }

  warning(message: string, horizontalPosition: MatSnackBarHorizontalPosition = 'center',
       verticalPosition: MatSnackBarVerticalPosition = 'bottom') {
    this.open({
      data: {
        message: message,
        icon: 'warning'
      },
      panelClass: 'warning-notification',
      horizontalPosition: horizontalPosition,
      verticalPosition: verticalPosition
    });
  }

  error(message: string, horizontalPosition: MatSnackBarHorizontalPosition = 'center',
        verticalPosition: MatSnackBarVerticalPosition = 'bottom') {
    this.open({
      data: {
        message: message,
        icon: 'error'
      },
      panelClass: 'error-notification',
      horizontalPosition: horizontalPosition,
      verticalPosition: verticalPosition
    });
  }

  private open(configuration: MatSnackBarConfig) {
    configuration.duration = 3000;

    this.zone.run(() => this.snackBar.openFromComponent(SnackBarTemplateComponent, configuration));
  }
}
