@echo off


git pull origin HEAD:master
git pull github HEAD:master

git push origin HEAD:master
git push github HEAD:master