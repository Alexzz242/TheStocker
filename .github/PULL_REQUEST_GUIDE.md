# Pull Request Guide — ThunderStrike Team

## The Golden Rule

Nobody pushes directly to `main`. All code goes through a Pull Request (PR). This protects the codebase and keeps everyone in sync.

---

## The Full Lifecycle

### 1. Start from a fresh `main`

Always make sure you're up to date before starting new work:

```bash
git checkout main
git pull origin main
```

### 2. Create a feature branch

Name it something descriptive:

```bash
git checkout -b feature/add-login-page
# or
git checkout -b fix/crash-on-startup
# or
git checkout -b chore/update-dependencies
```

### 3. Do your work and commit

Make small, focused commits as you go:

```bash
git add .
git commit -m "Add login form UI"
git commit -m "Connect login form to auth API"
```

Good commit messages finish the sentence: *"If applied, this commit will..."*

- ✅ `Add login form UI`
- ✅ `Fix null pointer crash on startup`
- ❌ `stuff`
- ❌ `wip`
- ❌ `fix`

### 4. Push your branch to GitHub

```bash
git push origin feature/add-login-page
```

### 5. Open a Pull Request on GitHub

1. Go to the repo on GitHub
2. You'll see a banner: **"Compare & pull request"** — click it
3. Fill out the PR template:
   - **Title**: short and descriptive — `Add login page` not `changes`
   - **What does this PR do?**: 2-3 sentences explaining the change
   - **Why?**: context for the reviewer
   - **How to test**: exact steps to verify it works
   - **Checklist**: tick the boxes honestly
4. Assign a **reviewer** (a teammate)
5. Click **Create Pull Request**

> 💡 Not ready for review yet? Click the dropdown arrow next to "Create Pull Request" and choose **"Create Draft Pull Request"** — this signals it's a work in progress.

### 6. The review process

**As the reviewer:**

- Read the description first, then the code
- Leave comments on specific lines by clicking the `+` next to a line
- Use comment types intentionally:
  - `nit:` — minor style thing, take it or leave it
  - `suggestion:` — here's a better way
  - `question:` — I don't understand this, can you explain?
  - `blocker:` — this needs to change before I approve
- When done, click **"Review changes"** and choose:
  - **Approve** — good to go
  - **Request changes** — needs work before merging
  - **Comment** — feedback only, no decision yet

**As the author:**

- Respond to every comment — either fix it or explain why you didn't
- Push new commits to address feedback (the branch auto-updates in the PR)
- Mark conversations as **Resolved** once addressed
- Re-request review when you're ready

### 7. Merge the PR

Once approved and all conversations resolved:

1. Click **"Squash and merge"** (this is the only allowed method)
2. Clean up the commit message if needed — it becomes one single commit on `main`
3. Click **"Confirm squash and merge"**
4. Click **"Delete branch"** — always clean up after yourself

### 8. Pull the latest `main`

After your PR merges (or a teammate's does), sync up:

```bash
git checkout main
git pull origin main
```

---

## Full Example

```bash
# Start fresh
git checkout main
git pull origin main

# Create branch
git checkout -b feature/mqtt-reconnect-logic

# Work, work, work...
git add .
git commit -m "Add exponential backoff for MQTT reconnect"
git add .
git commit -m "Add unit tests for reconnect logic"

# Push
git push origin feature/mqtt-reconnect-logic

# → Open PR on GitHub, fill template, assign reviewer
# → Reviewer leaves comments
# → You fix, push, resolve conversations
# → Reviewer approves
# → Squash and merge
# → Delete branch

# Sync up
git checkout main
git pull origin main
```

---

## Quick Reference

| Situation | Command |
|---|---|
| Start new work | `git checkout main && git pull origin main` |
| New branch | `git checkout -b feature/your-name` |
| Save progress | `git add . && git commit -m "message"` |
| Push branch | `git push origin feature/your-name` |
| Sync main | `git checkout main && git pull origin main` |

---

## What Will Get Your PR Rejected

- No description filled out
- Pushing directly to `main` (the branch rules will block this anyway)
- One giant PR with 10 different things changed — keep PRs small and focused
- Unresolved conflicts with `main`
