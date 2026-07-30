# Contributing Guidelines

## Code of Conduct

- Be respectful and professional in all interactions
- Provide constructive feedback
- Focus on code quality, not personal criticism
- Help others learn and grow
- Report issues responsibly

## Getting Started

1. Fork the repository
2. Clone your fork locally
3. Create a feature branch: `git checkout -b feature/feature-name`
4. Make your changes
5. Commit with conventional commits: `git commit -m "feat: description"`
6. Push to your fork: `git push origin feature/feature-name`
7. Create a Pull Request

## Branch Naming Convention

```
feature/feature-name          # New feature
fix/bug-description           # Bug fix
docs/documentation-topic      # Documentation
refactor/component-name       # Code refactoring
test/test-description         # Tests
chore/task-description        # Maintenance tasks
```

## Commit Message Format

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- **feat**: A new feature
- **fix**: A bug fix
- **docs**: Documentation only changes
- **style**: Changes that don't affect code meaning (whitespace, formatting)
- **refactor**: Code change that neither fixes a bug nor adds a feature
- **perf**: Code change that improves performance
- **test**: Adding or updating tests
- **chore**: Changes to build process, dependencies, tooling

### Examples

```
feat(posts): add ability to publish scheduled posts
fix(auth): resolve JWT token expiration issue
docs(setup): update installation instructions
refactor(core): simplify authentication service
test(posts): add unit tests for PostService
chore(deps): upgrade Angular to v16
```

## Pull Request Process

### Before Creating PR

1. **Ensure code quality**
   ```powershell
   # Run linter
   ng lint

   # Run tests
   dotnet test
   npm test
   ```

2. **Update documentation**
   - Update relevant .md files
   - Add/update code comments
   - Include examples if applicable

3. **Test thoroughly**
   - Test new features locally
   - Run full test suite
   - Check for console errors

### PR Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Related Issue
Fixes #(issue)

## Testing Performed
Describe the testing done

## Checklist
- [ ] Code follows style guidelines
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] No new warnings generated
- [ ] Tested on multiple browsers (frontend)
- [ ] Database migrations work (backend)
```

### PR Review Expectations

- **Response Time**: 24-48 hours
- **Approval Needed**: 2 reviewers for significant changes
- **CI/CD Must Pass**: All automated tests must pass
- **Code Coverage**: Maintain or improve coverage

## Code Quality Standards

### Test Coverage Requirements

| Project | Minimum | Target |
|---------|---------|--------|
| Backend (Core) | 70% | 80%+ |
| Frontend | 70% | 80%+ |
| Data Layer | 60% | 70%+ |

### Performance Requirements

- Backend API response: < 200ms (median)
- Frontend page load: < 2s
- Bundle size: < 500KB (gzipped)

### Security Requirements

- No hardcoded secrets
- OWASP compliance
- Dependency vulnerability checks
- No console.log in production code

## Documentation Requirements

### For New Features

1. **Code Documentation**
   ```csharp
   /// <summary>
   /// Description of what this does
   /// </summary>
   public async Task<Result> MethodAsync()
   ```

2. **README Update**
   - Feature overview
   - Usage examples
   - Configuration if needed

3. **API Documentation**
   - Endpoint documentation
   - Request/response examples
   - Error codes

## Development Workflow

### Setting Up Development Environment

```powershell
# 1. Clone repository
git clone https://github.com/your-org/blog-platform.git
cd blog-platform

# 2. Install backend dependencies
cd backend
dotnet restore

# 3. Create initial database
dotnet ef database update

# 4. Install frontend dependencies
cd ../frontend
npm install

# 5. Start development servers
# Terminal 1 - Backend
cd backend
dotnet watch run

# Terminal 2 - Frontend
cd frontend
ng serve
```

### Feature Development Steps

1. **Create feature branch**
   ```powershell
   git checkout -b feature/user-profile-page
   ```

2. **Write tests first (TDD)**
   - Unit tests
   - Integration tests
   - E2E tests

3. **Implement feature**
   - Follow architecture guidelines
   - Apply SOLID principles
   - Use existing patterns

4. **Local testing**
   ```powershell
   # Backend
   dotnet test

   # Frontend
   npm test
   ng e2e
   ```

5. **Create pull request**

### Bug Fix Workflow

1. **Create issue** (if not exists)
2. **Create branch** from main: `git checkout -b fix/issue-123`
3. **Write test** that reproduces bug
4. **Fix bug** with minimal changes
5. **Verify test** passes
6. **Submit PR** with issue reference

## Code Review Guidelines

### For Code Authors

- Keep PRs focused and reasonably sized (< 400 lines)
- Provide context in PR description
- Respond to feedback promptly
- Ask for clarification if needed
- Mark conversations as resolved once addressed

### For Code Reviewers

- Review within 24 hours if possible
- Be constructive and specific
- Approve if meets quality standards
- Test code locally if major changes
- Check for:
  - Code follows conventions
  - Tests are adequate
  - Security issues
  - Performance impacts
  - Documentation completeness

### Common Review Comments

✅ **Approve Pattern**
```
Looks good! A few small notes:
1. Consider extracting this method
2. Add test for edge case
Otherwise, ready to merge!
```

❌ **Request Changes Pattern**
```
This needs some updates:
1. Variable naming should follow convention
2. Missing unit test for this scenario
3. Security concern: SQL injection possible

Let me know when ready for re-review.
```

## Performance & Security Checklist

### Backend

- [ ] No N+1 queries
- [ ] Proper pagination implemented
- [ ] Error handling in place
- [ ] Input validation enforced
- [ ] SQL injection prevention
- [ ] CORS properly configured
- [ ] Rate limiting considered

### Frontend

- [ ] OnPush change detection used
- [ ] Unsubscribe from Observables
- [ ] No memory leaks
- [ ] XSS prevention
- [ ] CSRF tokens handled
- [ ] Sensitive data not in localStorage
- [ ] API errors handled gracefully

## Dependency Management

### Adding Dependencies

1. **Check for existing alternatives** in package.json
2. **Verify compatibility** with current versions
3. **Add to appropriate file**:
   - NuGet packages: .csproj files
   - NPM packages: package.json
4. **Update lock files**
5. **Document reason** for addition
6. **Run full test suite**

### Updating Dependencies

```powershell
# Backend
dotnet outdated
dotnet upgrade

# Frontend
npm outdated
npm update
```

## Release Process

### Pre-Release Checklist

- [ ] All PRs merged and tested
- [ ] Changelog updated
- [ ] Version bumped (semantic versioning)
- [ ] Documentation updated
- [ ] Database migrations tested
- [ ] Build succeeds
- [ ] Tests pass

### Semantic Versioning

Format: `MAJOR.MINOR.PATCH`

- `MAJOR`: Breaking changes
- `MINOR`: New features (backward compatible)
- `PATCH`: Bug fixes

Example: `v1.2.3`

## Communication

### Team Communication

- **Chat**: Use team Slack/Discord for quick discussions
- **Issues**: Use GitHub Issues for tracking work
- **PRs**: Use PRs for code discussion
- **Meetings**: Weekly sync for planning

### Asking for Help

1. Provide context and error messages
2. Share relevant code snippets
3. Mention what you've already tried
4. Ask specific questions

### Reporting Issues

Include:
- **Expected behavior**
- **Actual behavior**
- **Steps to reproduce**
- **Environment** (OS, versions)
- **Error messages/logs**
- **Screenshots** if applicable

## Useful Resources

- [Architecture Guide](./ARCHITECTURE.md)
- [Development Guidelines](./DEVELOPMENT_GUIDELINES.md)
- [API Specifications](./API_SPECS.md)
- [Frontend Structure](./FRONTEND_STRUCTURE.md)
- [Setup Guide](./SETUP.md)

## Questions?

- Read the documentation first
- Check existing issues/PRs
- Ask in team chat
- Create a new discussion if needed

---

**Last Updated**: July 2026
**Maintained By**: Development Team
