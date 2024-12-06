import { Component, OnInit, ViewChild, Output, EventEmitter, Input } from '@angular/core';
import { MatLegacyTableDataSource as MatTableDataSource } from '@angular/material/legacy-table';
import { MatSort, Sort } from '@angular/material/sort';
import { JJDispute, JJDisputeService } from 'app/services/jj-dispute.service';
import { LoggerService } from '@core/services/logger.service';
import { DisputeCaseFileSummary, PagedDisputeCaseFileSummaryCollection, SortDirection } from 'app/api';
import { AuthService } from 'app/services/auth.service';
import { LookupsService } from 'app/services/lookups.service';
import { TableFilter, TableFilterKeys } from '@shared/models/table-filter-options.model';
import { TableFilterService } from 'app/services/table-filter.service';
import { DisputeStatus } from '@shared/consts/DisputeStatus.model';
import { forkJoin, map } from 'rxjs';

@Component({
  selector: 'app-dispute-decision-inbox',
  templateUrl: './dispute-decision-inbox.component.html',
  styleUrls: ['./dispute-decision-inbox.component.scss'],
})
export class DisputeDecisionInboxComponent implements OnInit {
  @Input() tabIndex: number;
  courthouseTeamNames = ["A", "B", "C", "D"];

  @Output() jjDisputeInfo: EventEmitter<JJDispute> = new EventEmitter();
  @ViewChild(MatSort) sort = new MatSort();

  IDIR: string = "";
  courthouseTeams = {};
  tcoDisputes: DisputeCaseFileSummary[] = [];
  tcoDisputesCollection: PagedDisputeCaseFileSummaryCollection = {};
  dataSource = new MatTableDataSource(this.tcoDisputes);
  tableFilterKeys: TableFilterKeys[] = ["decisionDateFrom", "decisionDateTo", "occamDisputantName", "courthouseLocation", "ticketNumber", "team"];

  displayedColumns: string[] = [
    "ticketNumber",
    "jjDecisionDate",
    "signatoryName",
    "violationDate",
    "surname",
    "toBeHeardAtCourthouseName",
    "appearanceRoomCode",
    "status",
    "vtcAssignedTo"
  ];
  currentPage: number = 1;
  totalPages: number = 1;
  sortBy: string = "jjDecisionDate";
  sortDirection: SortDirection = SortDirection.Asc;
  filters: TableFilter = new TableFilter();
  disputeStatus = DisputeStatus;

  constructor(
    private logger: LoggerService,
    private authService: AuthService,
    private lookupsService: LookupsService,
    private tableFilterService: TableFilterService,
    private jjDisputeService: JJDisputeService
  ) {
    this.getCourthouseAgencyIds();
  }

  getCourthouseAgencyIds() {
    this.courthouseTeamNames.forEach(teamName => {
      let matchingTeams = this.lookupsService.courthouseTeams.filter(x => x.__team === teamName);
      let ids = matchingTeams.flatMap(team => 
        this.lookupsService.courthouseAgencies.filter(agency => agency.name.toLowerCase() === team.name.toLowerCase()).map(agency => agency.id));
      this.courthouseTeams[teamName] = ids;
    });
  }

  public ngOnInit() {
    let dataFilter: TableFilter = this.tableFilterService.tableFilters[this.tabIndex];
    dataFilter.status = dataFilter.status ?? "";
    this.filters = dataFilter;
    this.authService.userProfile$.subscribe(userProfile => {
      if (userProfile) {
        this.IDIR = userProfile.idir;
        this.getTCODisputes();
      }
    });
  }

  backWorkbench(value: JJDispute) {
    this.jjDisputeInfo.emit(value);
  }

  getTCODisputes(): void {
    this.logger.log('JJDisputeDecisionInboxComponent::getTCODisputes');

    const params = {
      appearances: true,
      timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone,
      jjDecisionDtFrom: this.filters.decisionDateFrom,
      jjDecisionDtThru: this.filters.decisionDateTo,
      ticketNumber: this.filters.ticketNumber ? this.filters.ticketNumber.toUpperCase() : "",
      surname: this.filters.occamDisputantName ?? "",
      disputeStatusCodes: DisputeStatus.Confirmed + "," + DisputeStatus.RequireCourtHearing,
      toBeHeardAtCourthouseIds: this.filters.courthouseLocation && this.filters.courthouseLocation.length > 0 ? 
        this.filters.courthouseLocation.map(x => x.id).join(",") : 
        (this.filters.team ? this.courthouseTeams[this.filters.team].join(",") : ""),
      sortBy: this.sortDirection === SortDirection.Asc ? this.sortBy : "-" + this.sortBy,
      pageNumber: this.currentPage,
      pageSize: 25
    };
    this.jjDisputeService.getTCODisputes(params).subscribe((response) => {
      this.tcoDisputes = [];
      this.logger.log('JJDisputeDecisionInboxComponent::getTCODisputes response');
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
}

