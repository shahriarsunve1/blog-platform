import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AdminService } from '../services/admin.service';
import { AdminDashboard } from '../../../shared/models/models';

@Component({
  selector: 'app-admin-dashboard',
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule]
})
export class AdminDashboardComponent implements OnInit {
  dashboard: AdminDashboard | null = null;
  isLoading = true;
  error: string | null = null;

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.adminService.getDashboard().subscribe({
      next: (response) => {
        this.dashboard = response.data ?? null;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading admin dashboard:', err);
        this.error = 'Failed to load dashboard data.';
        this.isLoading = false;
      }
    });
  }
}
