# 準備

## プロジェクトの準備

Unityでプロジェクトを新規作成

MRTKを導入

メニュー＞Mixed Reality Toolkit＞Configure＞Apply Mixed Reality 
Project Setting（チェックボックスはデフォルト）

Asset / HolographicAcademy フォルダを新規作成

## シーンの準備

Asset / Scenes フォルダを新規作成

Gazeシーンを新規作成

メニュー＞Mixed Reality Toolkit＞Configure＞Apply Mixed Reality 
Scene Setting（チェックボックスはデフォルト）

Directional Lightを削除

## 動作確認用3D Textを作成

Asset > HoloToolkit > UX > Prefabs > 3DTextPrefab 
をヒエラルキービューにドラッグ

新しい GameObject を右クリックして、名前を「Debug Log」に変更します。

Debug Log の Transform の Position を (0.1, 0, 2) にします。

Debug Log の Text Mesh コンポーネントの Anchor を Middle left 
にし、Alignment を Left にします。

# Gaze

## ヒット

### ねらい

視線の先（gaze）がヒットした地点の位置と法線を取得できるようにします

### MyGazeManager スクリプト

Asset / Scripts フォルダを新規作成

MyGazeManager スクリプトを新規作成

※コード

MyGazeManager スクリプトを managers オブジェクトにアタッチ

### 動作確認

[Hierarchy] パネル上部の [Create] メニューをクリックします。

[3D Object > Sphere] を選びます。

新しい GameObject を右クリックして、名前を「Test Sphere」に変更します。

Debug Log の Transform の Position を (0, 0, 2) に、Scale を (
0.1, 0.1, 0.1) にします。

DebugLogManager スクリプトを新規作成

※コード

DebugLog コンポーネントの My Text Mesh 欄に Text Mesh 
コンポーネントをドラッグします。

Playし、右クリックしたままドラッグして gaze の結果が変化するのを確認します。

gaze01.png

gaze02.png

## カーソル

### ねらい

次に、MyCusorManager.cs を編集して、以下のことを行います。

1. アクティブにするカーソルを決めます。

2. カーソルがホログラム上にあるかどうかに応じてカーソルを更新します。

3. ユーザーの視線の先にカーソルを位置付けます。

### MyCursorManager スクリプト

※コード

スクリプトを Managers オブジェクトにアタッチ

### 動作確認

Asset > HoloToolkit > Input > Prefabs > Cursor > 
Cursorプレハブをシーンに追加

Cursor オブジェクトの Object Cursor コンポーネントをオフにする

ヒエラルキービューのCursorを展開してCursorOnHologramsとCursorOffHologra
msを表示させる

MyCursorManagerコンポーネントのCursor On 
Holograms欄にCursorOnHologramsをドラッグし、Cursor Off 
Holograms欄にCursorOffHologramsをドラッグする

Playし、カーソルの形が変わるのを確かめる

※画像1

※画像2

## インタラクト

### ねらい

gaze したオブジェクトに自動的にメッセージを送り、オブジェクトの状態を変えます。

ここでは、gaze すると球の色が変わって音が鳴り、gaze 
が外れると元の色に戻って音が鳴るスクリプトを作成します。

### オブジェクトの取得

#### ねらい

InteractibleManager.cs 
は、視線のレイキャストがヒットした位置をフェッチし、ヒットした GameObject 
を保存します。

#### MyInteractibleManager スクリプト

※コード

スクリプトを Managers オブジェクトにアタッチ

#### 動作確認

#### DebugLog スクリプト

##### Update()

※コード

Playし、ヒットすると対象のオブジェクトの名前が取得できるのを確かめる

※画像1

※画像2

### メッセージの送信1

#### ねらい

この節は長いです。がんばって下さい。

視線の先に操作可能なオブジェクトがある場合は、GazeEntered 
メッセージを送信します。

操作可能なオブジェクトから視線がそれた場合は、GazeExited 
メッセージを送信します。

Interactible.cs では、GazeEntered コールバックと GazeExited 
コールバックを処理します。

#### InteractibleManager スクリプトの更新

##### Update()

※コード

##### region Private Methods

※コード

#### MyInteractible スクリプト

※コード

#### 動作確認

##### DebugLog スクリプト

###### Update()

※コード全部

Playし、ヒットすると対象のオブジェクトの名前が取得できるのを確かめる

※画像1

※画像2

### メッセージの送信2

#### ねらい

フォーカスすると色が変わって音が鳴るようにします

#### MyInteractible スクリプト

※コード

Test Sphere の My Interactible コンポーネントの Target Feedback 
Sound 欄に Asset > HoloToolkit > UX > Audio > Interaction > 
Button_Press.wav をドラッグ

#### マテリアルの編集

Test Sphere の Default-Material の Emission のチェックをオンにする

#### 動作確認

Playし、ヒットすると対象のオブジェクトが明るくなり、音が鳴る

※画像1

※画像2

### メッセージの送信3

#### ねらい

なるべくMRTKで用意されているスクリプトを使う

#### スクリプトのオンオフ

Managers の My Gaze Manager と My Cursor Manager と My 
Interactible Manager をオフにする

Managers に GazeManager をアタッチする

Cursor の Object Cursor をオンにする

#### MyInteractible スクリプト

##### region Public Methods

削除する

##### IPointerSpecificFocusable インターフェース

これを付けて、実装する

##### region IPointerSpecificFocusable CallBacks

※コード

#### MyGazeManager スクリプト

##### Update()

※コード

##### FocusedGameObjectName()

※コード

#### 動作確認

##### DebugLog スクリプト

Playし、ヒットすると対象のオブジェクトが明るくなり、音が鳴る

## ユーザーの方向を向く

### ねらい

あと少しです。オブジェクトが常にユーザーの方向を向くようにします。

### 作業

Test Sphereを選択し、右クリックで Cube を作り、Direction Cube 
という名前を付ける

Direction Cube の Transform を Position(0, 0, -0.5), Scale(
0.2, 0.2, 0.2) にする

Direction Cube の Mesh Renderer コンポーネントの Materials の 
Element 0 を DebugNormals にする

Test Sphere を選択します

[Inspector] パネルで [Add Component] をクリックします。

メニューの検索ボックスに「Billboard」と入力します。検索結果を選びます。

[Inspector] パネルで [Pivot Axis] を [Y] に設定します。

### 動作確認

Play し、右クリックしたまま WSAD 
キーで前後左右に移動することができます。移動しても Direction Cube 
が自分の方向を向いていることを確認して下さい。

## 追従

### ねらい

部屋の中でホログラムが自分の動きを追うようにする。

### 作業

Test Sphere に Tagalong スクリプトをアタッチする

Billboard コンポーネントの [Pivot Axis] を [free] に設定します。