import { Component, OnInit, ViewChild, AfterViewInit, Output, EventEmitter, Input } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatSort, Sort } from '@angular/material/sort';
import { CourthouseConfig } from '@config/config.model';
import { JJDisputeService, JJDisputeView } from 'app/services/jj-dispute.service';
import { JJDispute } from '../../../api/model/jJDispute.model';
import { MockConfigService } from 'tests/mocks/mock-config.service';
import { LoggerService } from '@core/services/logger.service';
import { Subscription } from 'rxjs';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { AuthService } from 'app/services/auth.service';
import { JJDisputeHearingType } from 'app/api';

@Component({
  selector: 'app-jj-dispute-wr-assignments',
  templateUrl: './jj-dispute-wr-assignments.component.html',
  styleUrls: ['./jj-dispute-wr-assignments.component.scss'],
})
export class JJDisputeWRAssignmentsComponent implements OnInit, AfterViewInit {
  @Output() public jjDisputeInfo: EventEmitter<JJDispute> = new EventEmitter();
  @Input() public IDIR: string;

  busy: Subscription;
  data = [] as JJDisputeView[];
  public courtLocations: CourthouseConfig[];
  public currentTeam: string = "A";
  public bulkjjAssignedTo: string = "unassigned";
  public HearingType = JJDisputeHearingType;
  public teamCounts: teamCounts[] = [];
  assignedDataSource = new MatTableDataSource();
  unassignedDataSource = new MatTableDataSource();
  @ViewChild(MatSort) sort = new MatSort();
  displayedColumns: string[] = [
    "assignedIcon",
    "jjAssignedTo",
    "bulkAssign",
    "ticketNumber",
    "submittedTs",
    "surname",
    "courthouseLocation",
    "policeDetachment",
    "timeToPayReason",
  ];

  constructor(
    public jjDisputeService: JJDisputeService,
    private logger: LoggerService,
    public mockConfigService: MockConfigService,
    public authService: AuthService

  ) {

    if (this.mockConfigService.courtLocations) {
      this.courtLocations = this.mockConfigService.courtLocations;
    }

    // listen for when to refresh from db
    this.jjDisputeService.refreshDisputes.subscribe(x => {
      this.getAll("A");
    });

  }

  ngOnInit(): void {
    this.getAll("A");
  }

  ngAfterViewInit() {
    this.assignedDataSource.sort = this.sort;
    this.unassignedDataSource.sort = this.sort;
  }

  sortData(event: Sort) {
    this.assignedDataSource.sort = this.sort;
  }

  backWorkbench(element) {
    this.jjDisputeInfo.emit(element);
  }

  getType(element: JJDispute): string {
    if (element.timeToPayReason && element.fineReductionReason)
      return "Time to pay/Fine";
    else if (element.timeToPayReason)
      return "Time to pay";
    else return "Fine";
  }

  filterByTeam(team: string) {
    let teamCourthouses = this.courtLocations.filter(x => x.jjTeam === team);
    this.assignedDataSource.data = this.data.filter(x => x.jjAssignedTo !== null && x.jjAssignedTo !== "unassigned" && teamCourthouses.filter(y => y.name === x.courthouseLocation).length > 0);
    this.unassignedDataSource.data = this.data.filter(x => (x.jjAssignedTo === null || x.jjAssignedTo === "unassigned") && teamCourthouses.filter(y => y.name === x.courthouseLocation).length > 0);
    this.currentTeam = team;
  }

  getCurrentTeamCounts(): teamCounts {
    return this.teamCounts.filter(x => x.team == this.currentTeam)[0];
  }

  getTeamCount(team: string): teamCounts {
    let teamCourthouses = this.courtLocations.filter(x => x.jjTeam === team);
    let teamDisputes = this.data.filter(x => teamCourthouses.filter(y => y.name === x.courthouseLocation).length > 0);
    let teamCounts = { team: team, assignedCount: 0, unassignedCount: 0 } as teamCounts;
    if (teamDisputes) {
      let unassignedTeamCounts = teamDisputes.filter(x => x.jjAssignedTo === null || x.jjAssignedTo === "unassigned");
      if (unassignedTeamCounts.length > 0) teamCounts.unassignedCount = unassignedTeamCounts.length;
      let assignedTeamCounts = teamDisputes.filter(x => x.jjAssignedTo !== null && x.jjAssignedTo !== "unassigned");
      if (assignedTeamCounts.length > 0) teamCounts.assignedCount = assignedTeamCounts.length;
      return teamCounts;
    }
    else return teamCounts;
  }

  getAll(team: string): void {
    this.logger.log('JJDisputeWRAssignmentsComponent::getAllDisputes');

    this.jjDisputeService.getJJDisputes().subscribe((response: JJDisputeView[]) => {
      // filter jj disputes only show new, review, in_progress
      this.data = response.filter(x => (this.jjDisputeService.JJDisputeStatusEditable.indexOf(x.status) >= 0) && x.hearingType === this.HearingType.WrittenReasons);
      this.data = this.data.sort((a: JJDisputeView, b: JJDisputeView) => { if (a.submittedTs > b.submittedTs) { return -1; } else { return 1 } });
      this.data.forEach(x => {
        x.jjAssignedToName = this.authService.getFullName(this.jjDisputeService.jjList?.filter(y => this.authService.getIDIR(y) === x.jjAssignedTo)[0]);
        x.bulkAssign = false;
      });
      this.resetAssignedUnassigned();

      this.filterByTeam(team); // initialize
    });
  }

  public resetAssignedUnassigned() {
    this.assignedDataSource.data = null; this.unassignedDataSource.data = null;
    this.assignedDataSource.data = this.data.filter(x => x.jjAssignedTo !== null && x.jjAssignedTo !== "unassigned" && x.jjAssignedTo !== undefined) as JJDispute[];
    this.unassignedDataSource.data = this.data.filter(x => x.jjAssignedTo === null || x.jjAssignedTo === "unassigned" || x.jjAssignedTo === undefined) as JJDispute[];
    this.unassignedDataSource.data.forEach((jjDispute: JJDispute) => {
      let index = this.unassignedDataSource.data.indexOf(jjDispute);
      jjDispute.jjAssignedTo = "unassigned";
      this.unassignedDataSource.data[index] = jjDispute;
    });

    this.teamCounts = [];
    this.teamCounts.push(this.getTeamCount("A"));
    this.teamCounts.push(this.getTeamCount("B"));
    this.teamCounts.push(this.getTeamCount("C"));
    this.teamCounts.push(this.getTeamCount("D"));

    this.filterByTeam(this.currentTeam);
  }

  public onAssign(element: JJDisputeView): void {
    this.bulkUpdateJJAssignedTo([element.ticketNumber], element.jjAssignedTo);
  }

  onSelectAll(event: MatCheckboxChange) {
    if (event.checked === true) {
      this.assignedDataSource.data.forEach((x: JJDisputeView) => x.bulkAssign = true);
      this.unassignedDataSource.data.forEach((x: JJDisputeView) => x.bulkAssign = true);
    } else {
      this.assignedDataSource.data.forEach((x: JJDisputeView) => x.bulkAssign = false);
      this.unassignedDataSource.data.forEach((x: JJDisputeView) => x.bulkAssign = false);
    }
  }

  bulkUpdateJJAssignedTo(ticketNumbers: string[], assignTo: string) {
    this.busy = this.jjDisputeService.apiJjAssignPut(ticketNumbers, assignTo).subscribe((response) => {
      this.logger.info(
        'JJDisputeWRAssignmentsComponent::onBulkAssign response',
        response
      );
      this.getAll("A");
      this.bulkjjAssignedTo = "unassigned";
    });
  }

  getBulkButtonDisabled() {
    if (this.assignedDataSource.data.filter((x: JJDisputeView) => x.bulkAssign === true)?.length == 0 &&
      this.unassignedDataSource.data.filter((x: JJDisputeView) => x.bulkAssign === true)?.length === 0)
      return true;
    else return false;
  }

  onBulkAssign() {
    let ticketNumbers = [];
    this.assignedDataSource.data.forEach((jjDispute: JJDisputeView) => {
      if (jjDispute.bulkAssign === true) ticketNumbers.push(jjDispute.ticketNumber);
    })
    this.unassignedDataSource.data.forEach((jjDispute: JJDisputeView) => {
      if (jjDispute.bulkAssign === true) ticketNumbers.push(jjDispute.ticketNumber);
    });
    this.bulkUpdateJJAssignedTo(ticketNumbers, this.bulkjjAssignedTo);
  }
}

export interface teamCounts {
  team: string;
  assignedCount: number;
  unassignedCount: number;
}
