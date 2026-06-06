#!/bin/sh
MSG=$(head -1 "$1")
PATTERN='^(build|chore|ci|docs|feat|fix|perf|refactor|revert|style|test)(\(.+\))?(!)?: .{1,}'

if ! echo "$MSG" | grep -qE "$PATTERN"; then
  echo ""
  echo "Invalid commit message: '$MSG'"
  echo ""
  echo "Must follow Conventional Commits format:"
  echo "  <type>(<scope>): <description>"
  echo ""
  echo "Valid types: build, chore, ci, docs, feat, fix, perf, refactor, revert, style, test"
  echo "Examples:"
  echo "  feat: add product search"
  echo "  fix(cart): prevent duplicate items"
  echo "  chore!: drop support for .NET 8"
  echo ""
  exit 1
fi
