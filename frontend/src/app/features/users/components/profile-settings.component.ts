import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../services/user.service';
import { AuthService } from '../../../core/auth/auth.service';
import { MediaService } from '../../posts/services/media.service';

const ALLOWED_IMAGE_TYPES = ['image/png', 'image/jpeg', 'image/gif', 'image/webp'];
const MAX_IMAGE_SIZE_BYTES = 5 * 1024 * 1024;

function passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {
  const newPassword = control.get('newPassword')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;
  return newPassword === confirmPassword ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-profile-settings',
  templateUrl: './profile-settings.component.html',
  styleUrls: ['./profile-settings.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class ProfileSettingsComponent implements OnInit {
  profileForm!: FormGroup;
  passwordForm!: FormGroup;

  isLoadingProfile = true;
  isSavingProfile = false;
  isSavingPassword = false;
  isSavingPreferences = false;
  isUploadingAvatar = false;

  avatarUrl = '';
  emailOnComment = true;
  emailOnFollow = true;

  profileError = '';
  profileSuccess = '';
  passwordError = '';
  passwordSuccess = '';
  preferencesError = '';
  preferencesSuccess = '';

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private authService: AuthService,
    private mediaService: MediaService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.profileForm = this.fb.group({
      username: ['', [Validators.required, Validators.maxLength(50)]],
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      bio: ['', [Validators.maxLength(500)]]
    });

    this.passwordForm = this.fb.group(
      {
        currentPassword: ['', Validators.required],
        newPassword: ['', [Validators.required, Validators.minLength(6)]],
        confirmPassword: ['', Validators.required]
      },
      { validators: passwordsMatchValidator }
    );

    this.loadProfile();
  }

  private loadProfile(): void {
    const userId = this.authService.getCurrentUser()?.id;
    if (!userId) {
      this.router.navigate(['/auth/login']);
      return;
    }

    this.isLoadingProfile = true;
    this.userService.getById(userId).subscribe({
      next: (response) => {
        const user = response.data;
        if (user) {
          this.profileForm.patchValue({
            username: user.username,
            firstName: user.firstName,
            lastName: user.lastName,
            bio: user.bio
          });
          this.avatarUrl = user.avatar;
          this.emailOnComment = user.emailOnComment ?? true;
          this.emailOnFollow = user.emailOnFollow ?? true;
        }
        this.isLoadingProfile = false;
      },
      error: () => {
        this.isLoadingProfile = false;
      }
    });
  }

  isFieldInvalid(form: FormGroup, fieldName: string): boolean {
    const field = form.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  get passwordsMismatch(): boolean {
    return !!this.passwordForm.errors?.['passwordMismatch'] && !!this.passwordForm.get('confirmPassword')?.touched;
  }

  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (!ALLOWED_IMAGE_TYPES.includes(file.type)) {
      this.profileError = 'Only PNG, JPEG, GIF, and WEBP images are supported';
      input.value = '';
      return;
    }
    if (file.size > MAX_IMAGE_SIZE_BYTES) {
      this.profileError = 'Image exceeds the 5MB size limit';
      input.value = '';
      return;
    }

    this.profileError = '';
    this.isUploadingAvatar = true;
    this.mediaService.upload(file).subscribe({
      next: (response) => {
        if (response.data) {
          this.avatarUrl = response.data.url;
        }
        this.isUploadingAvatar = false;
      },
      error: (err) => {
        this.profileError = err.error?.message || 'Failed to upload image';
        this.isUploadingAvatar = false;
      }
    });

    input.value = '';
  }

  saveProfile(): void {
    if (this.profileForm.invalid) return;

    this.isSavingProfile = true;
    this.profileError = '';
    this.profileSuccess = '';

    const request = {
      ...this.profileForm.value,
      avatar: this.avatarUrl
    };

    this.userService.updateProfile(request).subscribe({
      next: (response) => {
        if (response.data) {
          this.authService.updateCurrentUser(response.data);
        }
        this.profileSuccess = 'Profile updated successfully!';
        this.isSavingProfile = false;
      },
      error: (err) => {
        this.profileError = err.error?.message || 'Failed to update profile';
        this.isSavingProfile = false;
      }
    });
  }

  savePassword(): void {
    if (this.passwordForm.invalid) return;

    this.isSavingPassword = true;
    this.passwordError = '';
    this.passwordSuccess = '';

    const { currentPassword, newPassword } = this.passwordForm.value;
    this.userService.changePassword({ currentPassword, newPassword }).subscribe({
      next: () => {
        this.passwordSuccess = 'Password updated successfully!';
        this.passwordForm.reset();
        this.isSavingPassword = false;
      },
      error: (err) => {
        this.passwordError = err.error?.message || 'Failed to update password';
        this.isSavingPassword = false;
      }
    });
  }

  toggleEmailOnComment(): void {
    this.emailOnComment = !this.emailOnComment;
    this.savePreferences();
  }

  toggleEmailOnFollow(): void {
    this.emailOnFollow = !this.emailOnFollow;
    this.savePreferences();
  }

  private savePreferences(): void {
    this.isSavingPreferences = true;
    this.preferencesError = '';
    this.preferencesSuccess = '';

    this.userService
      .updatePreferences({
        emailOnComment: this.emailOnComment,
        emailOnFollow: this.emailOnFollow
      })
      .subscribe({
        next: () => {
          this.preferencesSuccess = 'Preferences saved';
          this.isSavingPreferences = false;
          setTimeout(() => (this.preferencesSuccess = ''), 2000);
        },
        error: (err) => {
          this.preferencesError = err.error?.message || 'Failed to update preferences';
          this.isSavingPreferences = false;
        }
      });
  }
}
