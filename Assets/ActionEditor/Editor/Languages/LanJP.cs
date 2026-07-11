

namespace PKC.ActionEditor
{
     [Name("日本語")]
       public class LanJP : ILanguages
       {

           //**********  Welcome *********
           public static string Title = "アクションタイムラインエディタ";
           public static string CreateAsset = "タイムラインを作成";
           public static string SelectAsset = "タイムラインを選択";
           public static string Setting = "エディタ設定";

           //**********  Create Window *********
           public static string CreateAssetType = "作成タイプ";
           public static string CreateAssetName = "タイムライン名";
           public static string CreateAssetFileName = "タイムラインのファイル名";
           public static string CreateAssetConfirm = "作成";
           public static string CreateAssetReset = "リセット";
           public static string CreateAssetTipsNameNull = "名前は空にできません";
           public static string CreateAssetTipsRepetitive = "同名のタイムラインが既に存在します";

           //**********  Preferences Window *********
           public static string PreferencesTitle = "エディタの設定";
           public static string PreferencesLanguage = "言語";
           public static string PreferencesTimeStepMode = "時間ステップモード";
           public static string PreferencesSnapInterval = "時間ステップ";
           public static string PreferencesFrameRate = "フレームレート";
           public static string PreferencesMagnetSnapping = "クリップスナッピング";
           public static string PreferencesMagnetSnappingTips = "クリップが他のクリップに自動的に吸着するかどうか";
           public static string PreferencesScrollWheelZooms = "ホイールズーム";
           public static string PreferencesScrollWheelZoomsTips = "タイムラインエリアのホイールズームを有効にするかどうか";
           public static string PreferencesSavePath = "設定保存先";
           public static string PreferencesSavePathTips = "作成と選択時のデフォルトの場所";
           public static string PreferencesAutoSaveTime = "自動保存時間";
           public static string PreferencesAutoSaveTimeTips = "定期的な自動保存操作の間隔時間";
           public static string PreferencesHelpDoc = "ヘルプドキュメント";

           //**********  Common *********
           public static string Select = "選択";
           public static string SelectFile = "ファイルを選択";
           public static string SelectFolder = "フォルダを選択";
           public static string TipsTitle = "ヒント";
           public static string TipsConfirm = "確認";
           public static string TipsCancel = "キャンセル";
           public static string CompilingTips = "コンパイル中\n...しばらくお待ちください...";
           public static string Disable = "無効";
           public static string Locked = "ロック";

           public static string Save = "保存";

           //**********  Header *********
           public static string HeaderLastSaveTime = "最終保存時間：{0}";
           public static string HeaderSelectAsset = "選択中：[{0}]";
           public static string OpenPreferencesTips = "設定画面を開く";
           public static string SelectAssetTips = "タイムラインを切り替えるにはクリック";
           public static string OpenMagnetSnappingTips = "クリップの磁気スナッピングを有効にする";
           public static string NewAssetTips = "新しいタイムラインを作成";
           public static string BackMenuTips = "メインメニューに戻る";
           public static string PlayLoopTips = "ループ再生";
           public static string PlayForwardTips = "最後にジャンプ";
           public static string StepForwardTips = "次のフレームにジャンプ";
           public static string PauseTips = "クリックして一時停止";
           public static string PlayTips = "クリックして再生";
           public static string StopTips = "クリックして再生停止";

           public static string StepBackwardTips = "前のフレームにジャンプ";

           //**********  Group Menu *********
           public static string MenuAddTrack = "トラックを追加";
           public static string MenuPasteTrack = "トラックを貼り付け";
           public static string GroupAdd = "グループを追加";
           public static string GroupDisable = "グループを無効にする";
           public static string GroupLocked = "グループをロック";
           public static string GroupReplica = "グループをコピー";
           public static string GroupDelete = "グループを削除";

           public static string GroupDeleteTips = "グループを削除してもよろしいですか?";

           //**********  Track Menu *********
           public static string TrackDisable = "トラック";
       }
}
