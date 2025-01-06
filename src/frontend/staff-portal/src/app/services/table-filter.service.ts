import { Injectable } from '@angular/core';
import { TableFilter } from '@shared/models/table-filter-options.model';

@Injectable({
  providedIn: 'root',
})
export class TableFilterService { // Temp
  tableFilters: TableFilter[] = new Array(4).fill(new TableFilter());
  currentPage: number[] = new Array(4).fill(1);
  constructor(
  ) {
  }
}