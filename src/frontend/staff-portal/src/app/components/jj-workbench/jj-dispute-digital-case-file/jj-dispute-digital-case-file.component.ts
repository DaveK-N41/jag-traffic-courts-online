import { Component, OnInit, ViewChild, AfterViewInit, Output, EventEmitter, Input } from '@angular/core';
import { MatLegacyTableDataSource as MatTableDataSource } from '@angular/material/legacy-table';
import { MatSort, Sort } from '@angular/material/sort';
import { JJDispute, JJDisputeService } from 'app/services/jj-dispute.service';
import { DisputeCaseFileSummary, JJDisputeHearingType, JJDisputeStatus, PagedDisputeCaseFileSummaryCollection, SortDirection } from 'app/api';
import { Observable, Subscription } from 'rxjs';
import { DateUtil } from '@shared/utils/date-util';
import { TableFilter, TableFilterKeys } from '@shared/models/table-filter-options.model';
import { TableFilterService } from 'app/services/table-filter.service';
import { LoggerService } from '@core/services/logger.service';

@Component({
  selector: 'app-jj-dispute-digital-case-file',
  templateUrl: './jj-dispute-digital-case-file.component.html',
  styleUrls: ['./jj-dispute-digital-case-file.component.scss']
})
export class JJDisputeDigitalCaseFileComponent implements OnInit {
  @Input() data$: Observable<JJDispute[]>;
  @Input() tabIndex: number;
  @Output() jjDisputeInfo: EventEmitter<JJDispute> = new EventEmitter();
  @ViewChild(MatSort) sort = new MatSort();

  HearingType = JJDisputeHearingType;
  jjAssignedToFilter: string;
  filterText: string;
  tcoDisputes: DisputeCaseFileSummary[] = [];
  tcoDisputesCollection: PagedDisputeCaseFileSummaryCollection = {};
  dataSource = new MatTableDataSource(this.tcoDisputes);
  tableFilterKeys: TableFilterKeys[] = ["dateSubmittedFrom", "dateSubmittedTo", "occamDisputantName", 
    "courthouseLocation", "ticketNumber"];

  displayedColumns: string[] = [
    "ticketNumber",
    "submittedTs",
    "violationDate",
    "disputantSurname",
    "toBeHeardAtCourthouseName",
    "disputeStatusDescription",
  ];
  currentPage: number = 1;
  totalPages: number = 1;
  sortBy: string = "submittedTs";
  sortDirection: SortDirection = SortDirection.Desc;
  filters: TableFilter = new TableFilter();
  jjDisputeStatus = JJDisputeStatus;

  constructor(
    private tableFilterService: TableFilterService,
    private jjDisputeService: JJDisputeService,
    private logger: LoggerService
  ) {
  }

  ngOnInit(): void {    
    let dataFilter: TableFilter = this.tableFilterService.tableFilters[this.tabIndex];
    dataFilter.status = dataFilter.status ?? "";
    this.filters = dataFilter; 
    this.getTCODisputes();    
  }

  getTCODisputes() {
    this.logger.log('JJDisputeDigitalCaseFileComponent::getTCODisputes');
    const params = {
      timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone,
      submittedFrom: this.filters.dateSubmittedFrom,
      submittedThru: this.filters.dateSubmittedTo,
      ticketNumber: this.filters.ticketNumber ? this.filters.ticketNumber.toUpperCase() : "",
      surname: this.filters.occamDisputantName ?? "",
      toBeHeardAtCourthouseIds: this.filters.courthouseLocation && this.filters.courthouseLocation.length > 0 ? 
        this.filters.courthouseLocation.map(x => x.id).join(",") : "",
      sortBy: this.sortDirection === SortDirection.Asc ? this.sortBy : "-" + this.sortBy,
      pageNumber: this.currentPage,
      pageSize: 25
    };
    this.jjDisputeService.getTCODisputes(params).subscribe((response) => {
      this.tcoDisputes = [];
      this.logger.log('JJDisputeDigitalCaseFileComponent::getTCODisputes response');
      this.tcoDisputesCollection = response;
      this.currentPage = response.pageNumber;
      this.totalPages = response.totalPages;
      if(!this.totalPages){
        this.currentPage = 0;
      }
      this.tcoDisputes = response.items;
      this.dataSource.data = this.tcoDisputes;
    });
  }

  backWorkbench(element) {
    this.jjDisputeInfo.emit(element);
  }

  onApplyFilter(dataFilters: TableFilter) {
    this.filters = dataFilters;
    this.currentPage = 1;
    this.getTCODisputes();
  }

  sortData(sort: Sort){
    this.sortBy = sort.active;
    this.sortDirection = sort.direction ? sort.direction as SortDirection : SortDirection.Desc;
    this.currentPage = 1;
    this.getTCODisputes();
  }

  onPageChange(event: number) {
    this.currentPage = event;
    this.getTCODisputes();
  }

  isConcludedStatus(status: JJDisputeStatus): boolean {
    const concludedStatuses = new Set([
      this.jjDisputeStatus.Accepted,
      this.jjDisputeStatus.Cancelled,
      this.jjDisputeStatus.Concluded
    ]);
    return concludedStatuses.has(status);
  }
}