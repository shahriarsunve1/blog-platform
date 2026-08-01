import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-privacy-policy',
  templateUrl: './privacy-policy.component.html',
  styleUrls: ['./privacy-policy.component.scss'],
  standalone: true,
  imports: [CommonModule]
})
export class PrivacyPolicyComponent implements OnInit {
  lastUpdated = '2026-08-01';

  constructor(private titleService: Title) {}

  ngOnInit(): void {
    this.titleService.setTitle('Privacy Policy | Resonate');
  }
}
