# Cluster Creator Kit ギミック ドキュメント

## SetGameObjectActiveGimmick

### 概要
SetGameObjectActiveGimmickは、GameObjectのアクティブ状態を切り替えるギミックコンポーネントです。

### 重要な仕様
- **Bool型のメッセージのみ受信可能**
- **true（on）でGameObjectがアクティブ化**
- **false（off）でGameObjectが非アクティブ化**
- **他のGameObjectを直接制御することはできない**（自分自身のみ）

### パラメータ
- **Key**: 受信するトリガーキー
- **Target**: Global / Item / LocalPlayer から選択
  - LocalPlayerの場合、Player Local UIの子である必要がある

### 動作の詳細
1. **Bool値による制御**
   - 送信側のParameterTypeがBoolである必要がある
   - boolValue: true → GameObject有効化
   - boolValue: false → GameObject無効化

2. **制御対象**
   - **自分自身のGameObjectのみ**制御可能
   - 他のオブジェクトを指定することはできない
   - 親子構造を使って間接的に制御する必要がある

### 使用上の注意
⚠️ **重要**: 自分自身を非アクティブにした場合、そのGameObjectは二度とメッセージを受信できなくなります。再度アクティブにすることはできません。

### 推奨される使用パターン

#### パターン1: 親子構造での制御
```
ParentObject（常にアクティブ）
├─ Item
├─ SetGameObjectActiveGimmick
│   └─ Target: Children
└─ DisplayContent（子・表示切り替え対象）
```

#### パターン2: 一方通行の非表示
- アイテム取得時の消滅
- エフェクトの一時表示

### ItemTimerとの組み合わせ

ItemTimerから送信されるトリガーの設定：
```yaml
triggers:
  - target: 4  # Global
    key: "ShowObject"
    type: 1  # Bool
    value:
      boolValue: 1  # true/false
```

### 現在のプロジェクトでの問題点

1. **別オブジェクトからの制御不可**
   - SetGameObjectActiveGimmickは自分自身しか制御できない
   - 01 Canvas、02 Canvas、03 Canvasを別オブジェクトから制御しようとしている

2. **解決策**
   - 各Canvas自身にSetGameObjectActiveGimmickを追加
   - 親子構造に変更して、親から子を制御
   - Animatorを使用した表示制御に変更

## PlayTimelineGimmick

### 概要
Unity Timelineを再生するギミック。Globalトリガーの送信が可能。

### 利点
- ItemTimerと異なり、Globalトリガーを送信可能
- 複数のオブジェクトを時間軸で制御可能
- Signal Trackを使用してトリガー送信

## ItemTimer

### 概要
指定時間後にトリガーを送信するギミック。

### 制限事項
- Triggersのtargetは**Itemのみ**（自分自身）
- Globalトリガーの直接送信は不可
- 他のItemへの直接送信も不可

### 使用方法
```yaml
globalGimmickKey:
  key:
    target: 2  # Global受信
    key: "Timer01"
delayTimeSeconds: 5
triggers:
  - target: 4  # 送信先（Itemは自分のみ）
    key: "TriggerKey"
    type: 1  # Bool
    value:
      boolValue: 1
```

## OnJoinPlayerTrigger

### 概要
プレイヤーがワールドに参加した時に発火するトリガー。

### 動作
- **Initialize Player Triggerの後に実行される**
- 新しいプレイヤーが参加するたびに発火
- 各プレイヤーごとに個別に動作
- DestroyItemGimmickでアイテムが破壊されていても実行される可能性がある

### 重要な制限事項
- **動的に生成されたアイテムでは実行されない**
- Itemコンポーネントが必須
- ワールド起動時ではなく、プレイヤー参加時に動作

### 使用例
```yaml
triggers:
  - target: 4  # Global
    key: "Timer01"  # 開始するタイマー
    type: 0  # Signal
```

### 推奨される用途
- ウェルカムメッセージの表示
- プレイヤー入室時のエフェクト
- 初期設定の開始トリガー

---

## Clusterにおける表示/非表示ループの制限事項

### なぜ複数オブジェクトの表示/非表示ループが不可能なのか

#### 1. SetGameObjectActiveGimmickの根本的制限
- **自分自身のGameObjectしか制御できない**
- 他のオブジェクトを指定する方法がない
- 一度非アクティブになると、そのオブジェクトは二度とメッセージを受信できない

#### 2. 親子構造での制御が機能しない理由
```
親オブジェクト
├─ SetGameObjectActiveGimmick
│   └─ Target: Children
└─ 子オブジェクト
```
**問題点：**
- SetGameObjectActiveGimmickに**Active（true/false）を設定する項目が存在しない**
- Bool値を受信しても、アクティブにするか非アクティブにするかを指定できない
- キー名による制御（Show/Hide）も機能しない

#### 3. Animatorでの制御が不可能な理由
- ClusterのAnimatorは**SetActive状態を制御できない**
- アニメーションプロパティ（位置、回転、スケール等）のみ制御可能
- GameObjectのアクティブ状態は変更できない

#### 4. Timeline + Signalの制限
```yaml
Timeline Signal Track:
  - type: 0 (Signal)のみ送信可能
  - type: 1 (Bool)を送信できない
```
**問題点：**
- SetGameObjectActiveGimmickは**Bool型（type: 1）のメッセージが必要**
- TimelineのSignalは**Signal型（type: 0）しか送信できない**
- 型の不一致により動作しない

#### 5. ItemTimerとSetGameObjectActiveGimmickの連携問題

**ItemTimerの制限：**
```yaml
triggers:
  - target: 0 (Item) → 自分自身のみ
  - target: 4 (Global) → 送信可能
```

**SetGameObjectActiveGimmickの受信：**
```yaml
globalGimmickKey:
  target: 0 (Item) → Itemキーを待つ
  target: 2 (Global) → Globalキーを待つ
```

**矛盾：**
- ItemTimerは他のItemに直接Bool値を送信できない（自分自身のみ）
- GlobalでBool値を送信しても、別オブジェクトのSetGameObjectActiveGimmickは自分自身しか制御できない

### 結論
**Clusterの現在の仕様では、複数のGameObjectを順番に表示/非表示してループさせることは構造的に不可能**

### 代替案
1. **VideoPlayerの動画切り替え**（GameObjectではなく動画を切り替える）
2. **位置移動**（SetActiveではなく、画面外への移動で対応）
3. **スケール変更**（0にして見えなくする）
4. **マテリアル変更**（透明にする）

---

## 推奨事項

### ビデオループシステムの実装
1. **PlayableDirector + Timeline**を使用
2. Play On Awakeで自動開始
3. Signal Trackでトリガー送信
4. 親子構造でCanvas制御

### デバッグ方法
1. Unityエディタでのプレイモードテスト
2. Clusterへのアップロードと実機テスト
3. コンソールログの確認

---

最終更新: 2025-08-23