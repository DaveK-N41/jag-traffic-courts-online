import { Pipe, PipeTransform } from "@angular/core";
import { DatePipe } from "@angular/common";

@Pipe({ name: "date" })
export class CustomDatePipe extends DatePipe implements PipeTransform {
  transform(value: Date | string | number, format?: string, timezone?: string, locale?: string): string | null;
  transform(value: null | undefined, format?: string, timezone?: string, locale?: string): null;
  transform(value: Date | string | number | null | undefined, format?: string, timezone?: string, locale?: string): string | null {
    let arr = format?.split("do"); // adding "do" to default DatePipe
    if (value) {
      let outputArr = arr.map(i => super.transform(value, i, timezone, locale));
      let separator = "";
      if (arr.length > 1) {
        let day = Number(super.transform(value, "d", timezone, locale));
        let suffix = "th";
        if (day % 100 < 11 || day % 100 > 13) {
          switch (day % 10) {
            case 1: suffix = "st"; break;
            case 2: suffix = "nd"; break;
            case 3: suffix = "rd"; break;
          }
        }
        separator = day + suffix;
      }
      return outputArr.join(separator);
    } else {
      return null;
    }
  }
}