import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { DisputeFormMode } from '@shared/enums/dispute-form-mode';
import { DisputeService, FileMetadata } from 'app/services/dispute.service';
import { NoticeOfDispute } from 'app/services/notice-of-dispute.service';
import { ViolationTicketService } from 'app/services/violation-ticket.service';
import { DisputeStore } from 'app/store';
import { BehaviorSubject, filter, map, Observable, take } from 'rxjs';

@Component({
  selector: 'app-update-dispute',
  templateUrl: './update-dispute.component.html',
  styleUrls: ['./update-dispute.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})

export class UpdateDisputeComponent implements OnInit {
  isLoaded$: BehaviorSubject<boolean> = new BehaviorSubject(false);
  mode: DisputeFormMode = DisputeFormMode.UPDATE;
  noticeOfDispute: NoticeOfDispute;
  ticketType: string;
  fileData$: Observable<FileMetadata[]>;

  constructor(
    private violationTicketService: ViolationTicketService,
    private disputeService: DisputeService,
    private store: Store,
  ) {
  }

  ngOnInit(): void {
    this.disputeService.checkStoredDispute().pipe(filter(i => !!i), take(1)).subscribe(found => {
      if (found) {
        this.store.select(DisputeStore.Selectors.NoticeOfDispute).pipe(filter(i => !!i)).subscribe(noticeOfDispute => {
          this.noticeOfDispute = noticeOfDispute;
          this.ticketType = this.violationTicketService.getTicketType(this.noticeOfDispute);
          this.isLoaded$.next(true);
        })
        this.fileData$ = this.store.select(DisputeStore.Selectors.FileData).pipe(
          map(i => {
            return i;
          })
        );
        this.store.dispatch(DisputeStore.Actions.Get());
      }
    })
  }

  /**
   * @description
   * Submit the dispute
   */
  public submitDispute(noticeOfDispute): void {
    this.store.dispatch(DisputeStore.Actions.Update({ payload: noticeOfDispute }));
  }
}
