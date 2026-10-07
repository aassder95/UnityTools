# TMP links

TmpLinkHandler: assign the TMP_Text in Inspector, enable raycastTarget, use an EventSystem/UI raycaster, subscribe/unsubscribe OnLinkClicked with your owner lifecycle. Touch/mouse position and camera come from PointerEventData, supporting overlay/camera/world canvases without parent searches. Only left/touch primary clicks are accepted. It emits the link ID; the application decides navigation or URL policy. It does not open external URLs automatically.
