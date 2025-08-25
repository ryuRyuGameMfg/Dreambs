# GitHubアップロード

以下の運用ルールに従って、GitHubへプロジェクトデータをアップロードしてください。

## 基本コマンドフロー

作業完了時は必ず以下のコマンドフローを実行してください：
```bash
# 1. 変更をステージング
git add .

# 2. コミット（カテゴリと動作確認状況を適切に設定）
git commit -m "[カテゴリ] 変更内容 - $(date +%Y-%m-%d) (動作確認状況)"

# 3. 現在のブランチにプッシュ
git push origin $(git branch --show-current)

# プッシュ完了通知
~/.claude/hooks/notification.sh "notification" "GitHubへのプッシュが完了しました"

# 4. 動作確認済みの場合のみ、mainブランチにマージ
# ※ (動作確認済み) のステータスの変更のみ実行可能
git checkout main
git merge $(git branch --show-current)
git push origin main

# 5. 元のブランチに戻る
git checkout $(git branch --show-current)
```

## ブランチ戦略の遵守

以下のブランチ戦略を必ず守ってください：
- **現在のブランチ**: 作業中のブランチ（全ての開発作業は現在のブランチで行う）
- **main**: 本番ブランチ（動作確認済みのコードのみマージ）

## コミットメッセージ規則の厳守

コミットメッセージは必ず以下の形式で記述してください：

```
[カテゴリ] 変更内容の説明 - 更新日時 (動作確認状況)
```

### カテゴリ一覧（必須選択）
以下から適切なカテゴリを選択してください：
- `[add]`: 新機能・新ファイル追加
- `[fix]`: バグ修正
- `[refactor]`: リファクタリング・構造変更
- `[remove]`: ファイル・機能削除
- `[update]`: 既存機能の更新・改善
- `[docs]`: ドキュメント更新
- `[style]`: コードフォーマット・整理

### 動作確認状況（必須記載）
以下から適切なステータスを選択してください：
- `(確認済み)`: ドキュメント更新、設定変更、ファイル削除等、実行不要な変更
- `(未テスト)`: 実装・修正後のデフォルト状態、人間による確認待ち
- `(動作確認済み)`: 人間が動作確認済み、mainブランチへのマージ可能

## 作業シーン別の実行指示

### 1. 新機能開発時の手順
以下の手順を必ず実行してください：
```bash
# 現在のブランチで作業を行う

# 変更を行い、コミット
git add .
git commit -m "[add] 新機能の実装 - $(date +%Y-%m-%d) (未テスト)"

# 現在のブランチにプッシュ
git push origin $(git branch --show-current)

# プッシュ完了通知
~/.claude/hooks/notification.sh "notification" "GitHubへのプッシュが完了しました"
```

### 2. バグ修正時の手順
以下の手順を必ず実行してください：
```bash
# 現在のブランチで作業を行う

# 変更を行い、コミット
git add .
git commit -m "[fix] バグの説明 - $(date +%Y-%m-%d) (未テスト)"

# 現在のブランチにプッシュ
git push origin $(git branch --show-current)

# プッシュ完了通知
~/.claude/hooks/notification.sh "notification" "GitHubへのプッシュが完了しました"
```

### 3. 動作確認後のmainマージ手順
動作確認が完了した場合のみ、以下を実行してください：
```bash
# 現在のブランチ名を保存
CURRENT_BRANCH=$(git branch --show-current)

# 動作確認が完了したら、コミットメッセージを更新
git commit --amend -m "[fix] バグの説明 - $(date +%Y-%m-%d) (動作確認済み)"
git push origin $CURRENT_BRANCH --force-with-lease

# mainブランチにマージ
git checkout main
git merge $CURRENT_BRANCH
git push origin main

# mainブランチへのマージ完了通知
~/.claude/hooks/notification.sh "notification" "mainブランチへのマージが完了しました"

# 元のブランチに戻る
git checkout $CURRENT_BRANCH
```

### 4. リファクタリング時の手順
以下の手順を必ず実行してください：
```bash
# 現在のブランチで作業を行う

# リファクタリング実施
git add .
git commit -m "[refactor] システム構造の改善 - $(date +%Y-%m-%d) (未テスト)"
git push origin $(git branch --show-current)
```

### 5. 不要ファイル削除時の手順
以下の手順を必ず実行してください：
```bash
# 現在のブランチで作業を行う

# ファイル削除
git rm 不要なファイル.cs
git rm 不要なファイル.cs.meta
git commit -m "[remove] 未使用ファイルの削除 - $(date +%Y-%m-%d) (確認済み)"
git push origin $(git branch --show-current)
```

## 便利なエイリアス設定（推奨）

効率化のため、以下のエイリアスを設定することを推奨します：
```bash
# Git設定に追加すると便利なエイリアス
git config --global alias.st status
git config --global alias.co checkout
git config --global alias.br branch
git config --global alias.cm commit
git config --global alias.pl pull
git config --global alias.ps push
git config --global alias.lg "log --oneline --graph --all"
```

## 絶対に守るべき禁止事項

以下の行為は絶対に行わないでください：
1. **mainブランチに直接プッシュしない**
2. **動作確認なしでmainにマージしない**
3. **コミットメッセージは日本語で明確に記述**
4. **metaファイルも忘れずに削除・追加**
5. **APIキーなどの機密情報は絶対にコミットしない**
