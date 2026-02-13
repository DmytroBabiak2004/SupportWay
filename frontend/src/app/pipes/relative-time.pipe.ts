import { Pipe, PipeTransform } from '@angular/core';
import { DatePipe } from '@angular/common';

@Pipe({
  name: 'relativeTime',
  standalone: true
})
export class RelativeTimePipe implements PipeTransform {

  transform(value: string | Date | undefined): string {
    if (!value) return '';
    let dateInput = value;
    if (typeof value === 'string' && value.indexOf('Z') === -1 && value.indexOf('+') === -1) {

    }

    const date = new Date(dateInput);
    const now = new Date();

    let seconds = Math.floor((now.getTime() - date.getTime()) / 1000);

    if (seconds < 0) {
      seconds = 0;
    }

    if (seconds < 60) {
      return 'щойно';
    }

    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);
    const days = Math.floor(hours / 24);

    if (minutes < 60) {
      return `${minutes} хв. тому`;
    }

    if (hours < 24) {
      return `${hours} год. тому`;
    }


    if (days > 7) {
      const datePipe = new DatePipe('uk-UA');
      return datePipe.transform(date, 'd MMM HH:mm') || '';
    }

    return `${days} дн. тому`;
  }
}
