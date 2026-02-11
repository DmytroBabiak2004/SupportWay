import { Pipe, PipeTransform } from '@angular/core';
import { DatePipe } from '@angular/common';

@Pipe({
  name: 'relativeTime',
  standalone: true
})
export class RelativeTimePipe implements PipeTransform {

  transform(value: string | Date | undefined): string {
    if (!value) return '';

    const date = new Date(value);
    const now = new Date();
    const seconds = Math.floor((now.getTime() - date.getTime()) / 1000);

    if (seconds < 60) {
      return 'щойно';
    }

    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);

    if (minutes < 60) {
      return `${minutes} хв. тому`;
    }

    if (hours < 24) {
      return `${hours} год. тому`;
    }

    const datePipe = new DatePipe('uk-UA');

    return datePipe.transform(value, 'd MMM HH:mm') || '';
  }
}
