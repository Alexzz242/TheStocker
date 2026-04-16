git checkout master
git pull origin master

# Create a feature branch
git checkout -b feature/add-inventory-system
#or
git checkout -b fix/player-movement-bug
#or
git checkout -b chore/update-godot-version
#or
git checkout -b refactor/ui-cleanup

# Do your work and commit
git add .
git commit -m "Add inventory UI scene"
git commit -m "Connect inventory to player data script"

git push origin feature/add-inventory-system

# Situation,Command
Start new work-> git checkout master && git pull origin master
New branch-> git checkout -b feature/your-feature-name
Save progress-> "git add . && git commit -m ""message"""
Push branch-> git push origin feature/your-feature-name
Sync master-> git checkout master && git pull origin master
