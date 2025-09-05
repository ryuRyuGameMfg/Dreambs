// ClusterScript（Scriptable Item）
// 重要：ClusterScriptから直接Bool値を送信してSetGameObjectActiveGimmickを
// 制御することは仕様上不可能です

const nodeNames = ["VP1", "VP2", "VP3", "VP4"];     // VideoPlayer を持つオブジェクト
const lengthsSec = [15, 15, 15, 15];          // 各クリップの尺（秒）
const initialBufferSec = 10;                 // 初回バッファ（URLソースなら大きめに）

let idx = 0;
let t = 0;

$.onStart(() => {                             
  idx = 0;
  t = -initialBufferSec;                      
  
  $.log("=== VideoSequencePlayer 開始 ===");
  // Signal型のトリガーを送信（Bool型ではない）
  sendVideoSignal(0);
});

// Signal型のトリガーを送信（SetGameObjectActiveGimmickでは使えない）
function sendVideoSignal(activeIndex) {
  // Signalトリガーを送信（他のギミックで受信可能）
  const signalKey = `Video_${activeIndex + 1}`;
  $.sendSignal("global", signalKey);
  $.log(`Signal送信: ${signalKey} (VP${activeIndex + 1}用)`);
}

$.onUpdate((dt) => {                          
  t += dt;                                    
  const len = lengthsSec[idx] ?? 0;
  
  // デバッグ用：5秒ごとに進捗表示
  if (Math.floor(t) % 5 === 0 && Math.abs(t - Math.floor(t)) < dt) {
    $.log(`[進捗] ${nodeNames[idx]}: ${Math.floor(t)}/${len}秒`);
  }
  
  if (t >= len) {
    t = 0;
    const prevIdx = idx;
    idx = (idx + 1) % nodeNames.length;       
    
    $.log(`=== 切替: ${nodeNames[prevIdx]} → ${nodeNames[idx]} ===`);
    
    // Signalを送信
    sendVideoSignal(idx);
  }
});

/* 
重要な制限事項：

1. ClusterScriptの$.sendSignal()は**Signal型（type: 0）のみ**送信可能
2. SetGameObjectActiveGimmickは**Bool型（type: 1）が必要**
3. つまり、ClusterScriptから直接SetGameObjectActiveGimmickは制御不可

解決策：

Unity側で以下のいずれかの方法を使用：

方法1: ItemTimerを中継
- ClusterScriptのSignalでItemTimerを起動
- ItemTimerからBool値を送信してSetGameObjectActiveGimmickを制御
- ただし、ItemTimerは自分自身のItemにしかBool値を送信できない

方法2: SetPositionGimmickを使用（推奨）
- 各VPにSetPositionGimmickを追加
- GlobalGimmickKey: "Video_1", "Video_2"など（Signal型で受信可能）
- 表示位置と画面外位置を設定

方法3: PlayTimelineGimmickを使用
- TimelineでVPの表示/非表示をアニメーション制御
- GlobalGimmickKeyでSignalを受信してTimeline再生

方法4: 動画URLを動的に変更
- 1つのVideoPlayerで複数の動画を切り替える
- ただし、ClusterScriptからURL変更は不可

現実的な解決策：
SetPositionGimmickで画面内/画面外への移動で表示制御するのが最も確実
*/