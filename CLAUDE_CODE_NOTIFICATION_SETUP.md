# Claude Code 通知音設定ガイド

ClaudeCodeでタスク完了時にmacOSの通知音を鳴らす設定方法です。

## 概要
ClaudeCodeのHooks機能を使用して、タスク完了時に自動的にmacOSのシステム音を再生します。

## セットアップ手順

### 1. 通知スクリプトの作成

`~/.claude/hooks/notification.sh`を作成：

```bash
#!/bin/bash

# Claude Code notification hook for task completion
# This hook shows macOS notifications with sound when tasks are completed

# Debug logging (オプション)
LOG_FILE="/tmp/claude_notification.log"
echo "$(date): Hook called with EVENT_TYPE=$1" >> "$LOG_FILE"

# Get the hook event type from environment or arguments
EVENT_TYPE="${CLAUDE_HOOK_EVENT:-$1}"
MESSAGE="${2:-タスクが完了しました}"

echo "$(date): EVENT_TYPE=$EVENT_TYPE, MESSAGE=$MESSAGE" >> "$LOG_FILE"

# Function to show notification with sound
show_notification() {
    # Available sounds:
    # Basso, Blow, Bottle, Frog, Funk, Glass, Hero, Morse, 
    # Ping, Pop, Purr, Sosumi, Submarine, Tink
    
    SOUND_NAME="Hero"  # 変更可能: お好みのサウンドに変更してください
    
    echo "$(date): Showing notification with sound $SOUND_NAME" >> "$LOG_FILE"
    
    # Play sound directly first (more reliable)
    # -v flag sets volume (0.0 to 1.0, default is 1.0)
    afplay -v 1.0 "/System/Library/Sounds/${SOUND_NAME}.aiff" &
    
    # Also try to show macOS notification with sound
    osascript -e "display notification \"$MESSAGE\" with title \"Claude Code\" sound name \"$SOUND_NAME\"" 2>> "$LOG_FILE"
}

# Check if this is a task completion event
case "$EVENT_TYPE" in
    "PostToolUse"|"SessionEnd"|"task_complete"|"todo_complete"|"notification")
        echo "$(date): Matched event type, calling show_notification" >> "$LOG_FILE"
        show_notification
        ;;
    *)
        echo "$(date): No match for event type: $EVENT_TYPE" >> "$LOG_FILE"
        # For other events, do nothing
        ;;
esac

# Always exit successfully to not block Claude Code
exit 0
```

### 2. スクリプトに実行権限を付与

```bash
chmod +x ~/.claude/hooks/notification.sh
```

### 3. プロジェクト設定ファイルの更新

`.claude/settings.local.json`に以下を追加：

```json
{
  "permissions": {
    "allow": [
      // 既存の権限設定...
      "Bash(chmod:*)",
      "Bash(~/.claude/hooks/notification.sh:*)",
      "Read(//System/Library/Sounds/**)",
      "Bash(afplay:*)",
      "Bash(osascript:*)"
    ]
  },
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "TodoWrite",
        "hooks": [
          {
            "type": "command",
            "command": "~/.claude/hooks/notification.sh PostToolUse"
          }
        ]
      }
    ],
    "SessionEnd": [
      {
        "matcher": "*",
        "hooks": [
          {
            "type": "command",
            "command": "~/.claude/hooks/notification.sh SessionEnd"
          }
        ]
      }
    ]
  }
}
```

## カスタマイズ

### 通知音の変更

`notification.sh`の`SOUND_NAME`変数を変更：

| サウンド名 | 説明 |
|-----------|------|
| Basso | 低音の警告音 |
| Blow | 風の音 |
| Bottle | ボトルの音 |
| Frog | カエルの鳴き声 |
| Funk | ファンキーな音 |
| Glass | ガラスの音 |
| Hero | 英雄的な音（デフォルト） |
| Morse | モールス信号風 |
| Ping | ピン音 |
| Pop | ポップ音 |
| Purr | 猫の鳴き声 |
| Sosumi | そう済み |
| Submarine | 潜水艦の音 |
| Tink | チンク音 |

### 音量の調整

`afplay -v`の値を変更（0.0〜1.0）：
```bash
afplay -v 0.5 "/System/Library/Sounds/${SOUND_NAME}.aiff"  # 50%の音量
```

### 通知タイミングの変更

#### 推奨設定（ファイル操作完了時）

**実際の成果物作成・変更時のみ音を鳴らす設定：**

```json
"hooks": {
  "PostToolUse": [
    {
      "matcher": "Write",
      "hooks": [
        {
          "type": "command",
          "command": "~/.claude/hooks/notification.sh PostToolUse Write"
        }
      ]
    },
    {
      "matcher": "Edit",
      "hooks": [
        {
          "type": "command",
          "command": "~/.claude/hooks/notification.sh PostToolUse Edit"
        }
      ]
    },
    {
      "matcher": "MultiEdit",
      "hooks": [
        {
          "type": "command",
          "command": "~/.claude/hooks/notification.sh PostToolUse MultiEdit"
        }
      ]
    }
  ],
  "SessionEnd": [
    {
      "matcher": "*",
      "hooks": [
        {
          "type": "command",
          "command": "~/.claude/hooks/notification.sh SessionEnd"
        }
      ]
    }
  ]
}
```

この設定により：
- **Write**: 新規ファイル作成時に音が鳴る
- **Edit**: ファイル編集時に音が鳴る
- **MultiEdit**: 複数編集時に音が鳴る
- **SessionEnd**: セッション終了時に音が鳴る

#### TodoWriteツールについて

**TodoWriteとは：**
ClaudeCodeの内部タスク管理ツール。以下のすべての操作で呼ばれます：
- タスク作成
- ステータス変更（pending → in_progress → completed）
- タスク更新
- タスク削除

**なぜTodoWriteを推奨しないか：**
- タスク完了時だけでなく、すべての操作で音が鳴ってしまう
- 頻繁に音が鳴りすぎて実用的でない
- 現在のClaudeCode仕様では、完了操作のみを区別できない

**結論：**
ファイル操作（Write/Edit/MultiEdit）の完了時に音を鳴らす方が、実質的な「タスク完了」として最適です。

#### 利用可能なフックポイント

| フックポイント | 説明 | 推奨度 |
|--------------|------|--------|
| PreToolUse | ツール使用前 | ❌ 頻繁すぎる |
| PostToolUse | ツール使用後 | ⭕ 特定ツールのみ推奨 |
| TodoWrite | タスク管理操作時 | ❌ すべての操作で鳴る |
| Write/Edit | ファイル作成・編集時 | ✅ 推奨 |
| Bash | コマンド実行時 | △ 必要に応じて |
| UserPromptSubmit | ユーザー入力時 | ❌ 不要 |
| SessionStart | セッション開始時 | △ 必要に応じて |
| SessionEnd | セッション終了時 | ⭕ 推奨 |

## テスト方法

### 1. 直接実行テスト
```bash
~/.claude/hooks/notification.sh notification "テスト通知"
```

### 2. ClaudeCode内でのテスト
TodoWriteツールを使用してタスクを作成・完了

### 3. デバッグログの確認
```bash
cat /tmp/claude_notification.log
```

## トラブルシューティング

### 音が鳴らない場合

1. **システム音量の確認**
   - macOSのサウンド設定を確認
   - ミュートになっていないか確認

2. **権限の確認**
   ```bash
   ls -la ~/.claude/hooks/notification.sh
   ```
   実行権限（x）があることを確認

3. **フックが呼ばれているか確認**
   ```bash
   tail -f /tmp/claude_notification.log
   ```

4. **macOSの通知設定**
   - システム設定 > 通知
   - 集中モードがオフになっているか確認

5. **サウンドファイルの存在確認**
   ```bash
   ls /System/Library/Sounds/
   ```

## 注意事項

- ClaudeCodeの各セッションで設定が有効になります
- プロジェクトごとに`.claude/settings.local.json`の設定が必要です
- グローバル設定は`~/.claude/settings.json`に記述できます

## 関連リンク

- [Claude Code Hooks Documentation](https://docs.anthropic.com/en/docs/claude-code/hooks-overview)
- [macOS Audio System](https://developer.apple.com/documentation/avfaudio)

---

作成日: 2025-08-25
動作確認環境: macOS, Claude Code with Opus 4.1