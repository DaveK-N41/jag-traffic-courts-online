import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { DisputeDisputantDetectedOcrIssues, ViolationTicket, ViolationTicketCount } from 'app/api';
import { ViolationTicketService } from 'app/services/violation-ticket.service';

@Component({
  selector: 'app-scan-ticket',
  templateUrl: './scan-ticket.component.html',
  styleUrls: ['./scan-ticket.component.scss'],
})
export class ScanTicketComponent implements OnInit {
  private ticket: ViolationTicket;
  ticketImageSrc: SafeUrl;
  ticketImageFile: string;
  ticketFilename: string;
  form: FormGroup;
  DetectedOcrIssues = DisputeDisputantDetectedOcrIssues;
  violationTicketCounts: ViolationTicketCount[] = [];

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private violationTicketService: ViolationTicketService,
    private sanitizer: DomSanitizer
  ) {
    this.router.routeReuseStrategy.shouldReuseRoute = () => { return false; };
  }

  ngOnInit(): void {
    let inputTicketData = this.violationTicketService.inputTicketData;
    this.ticket = this.violationTicketService.ticket;
    if (!inputTicketData || !this.ticket) {
      this.violationTicketService.goToFind();
      return;
    }

    // get count summary info
    this.ticket.counts.forEach(count => {
      this.violationTicketCounts.push(count);
    });
    this.violationTicketCounts = this.violationTicketCounts.sort((a,b)=> a.count_no - b.count_no);

    this.ticketImageSrc = this.sanitizer.bypassSecurityTrustUrl(inputTicketData.ticketImage);
    this.ticketFilename = inputTicketData.filename;
    this.ticketImageFile = inputTicketData.ticketFile.type
    this.form = this.formBuilder.group(this.ticket); // can add control
    this.form.disable();
    this.form.controls.disputant_detected_ocr_issues.enable();
    this.form.controls.disputant_ocr_issues.enable();
    this.form.controls.disputant_ocr_issues.addValidators([Validators.maxLength(500)]);
    this.form.controls.disputant_ocr_issues.updateValueAndValidity();
  }

  onSubmit(): void {
    this.violationTicketService.updateOcrIssue(this.form.value.disputant_detected_ocr_issues, this.form.value.disputant_ocr_issues);
    this.violationTicketService.goToInitiateResolution();
  }
}
