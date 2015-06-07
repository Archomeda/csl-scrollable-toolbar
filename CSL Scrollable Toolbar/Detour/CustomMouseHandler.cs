using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ColossalFramework.UI;
using CommonShared.Utils;
using ScrollableToolbar.Utils;
using UnityEngine;

namespace ScrollableToolbar.Detour
{
    /// <summary>
    /// For some reason (perfectly valid actually), UI controls don't see the event OnMouseWheel being fired when they're disabled.
    /// In order to get a natural feeling in the toolbar, we have to enable this however.
    /// This only requires a small bit of code to be rewritten, but it involves a HUGE bit of copy-paste code.
    /// If you feel lucky, go ahead and try to understand the code. I probably won't even recognize what I did in a few weeks...
    /// </summary>
    internal static class CustomMouseHandler
    {
        private static readonly MethodInfo processInputOriginal = typeof(UIInput.MouseHandler).GetMethod("ProcessInput");
        private static readonly MethodInfo processInputReplacement = typeof(CustomMouseHandler).GetMethod("ProcessInput");
        private static DetourCallsState processInputState;

        public static void Detour()
        {
            try
            {
                processInputState = DetourUtils.RedirectCalls(processInputOriginal, processInputReplacement);
                Mod.Log.Info("UIInput.MouseHandler.ProcessInput() has been detoured");
            }
            catch (Exception ex)
            {
                Mod.Log.Error("Exception while detouring UIInput.MouseHandler.ProcessInput(): {0}", ex);
            }
        }

        public static void UnDetour()
        {
            try
            {
                DetourUtils.RevertRedirect(processInputOriginal, processInputState);
                Mod.Log.Info("UIInput.MouseHandler.ProcessInput() detour has been reverted");
            }
            catch (Exception ex)
            {
                Mod.Log.Error("Exception while reverting detour UIInput.MouseHandler.ProcessInput(): {0}", ex);
            }
        }

        private static UIDragEventParameter CreateUIDragEventParameter(params object[] args)
        {
            return (UIDragEventParameter)Activator.CreateInstance(typeof(UIDragEventParameter), BindingFlags.Instance | BindingFlags.NonPublic, null, args, null);
        }


        public static void ProcessInput(UIInput.MouseHandler @this, IInputTranslator translator, Ray ray, UIComponent component, bool retainFocusSetting)
        {
            // This method was really spooky, and now it's beyond spooky
            // Don't try this at home kids, even I don't know how this works.

            // Get all used private variables on @this (these need to be saved back to the object at some point, otherwise we risk breaking things)
            UIMouseButton m_ButtonsPressed = ReflectionUtils.GetPrivateField<UIMouseButton>(@this, "m_ButtonsPressed");
            UIMouseButton m_ButtonsUp = ReflectionUtils.GetPrivateField<UIMouseButton>(@this, "m_ButtonsUp");
            UIMouseButton m_ButtonsDown = ReflectionUtils.GetPrivateField<UIMouseButton>(@this, "m_ButtonsDown");
            Vector2 m_LastPosition = ReflectionUtils.GetPrivateField<Vector2>(@this, "m_LastPosition");
            Vector2 m_MouseMoveDelta = ReflectionUtils.GetPrivateField<Vector2>(@this, "m_MouseMoveDelta");
            UIDragDropState m_DragState = ReflectionUtils.GetPrivateField<UIDragDropState>(@this, "m_DragState");
            UIComponent m_ActiveComponent = ReflectionUtils.GetPrivateField<UIComponent>(@this, "m_ActiveComponent");
            UIComponent m_LastDragComponent = ReflectionUtils.GetPrivateField<UIComponent>(@this, "m_LastDragComponent");
            object m_DragData = ReflectionUtils.GetPrivateField<object>(@this, "m_DragData");
            float m_LastClickTime = ReflectionUtils.GetPrivateField<float>(@this, "m_LastClickTime");
            float m_LastHoverTime = ReflectionUtils.GetPrivateField<float>(@this, "m_LastHoverTime");

            // Original code follows, with modifications to allow it run in our context
            Vector2 mousePosition = translator.GetMousePosition();
            m_ButtonsPressed = UIMouseButton.None;
            m_ButtonsUp = UIMouseButton.None;
            m_ButtonsDown = UIMouseButton.None;
            object[] args = { translator, m_ButtonsPressed, m_ButtonsUp, m_ButtonsDown };
            ReflectionUtils.InvokePrivateStaticMethod(typeof(UIInput.MouseHandler), "GetMouseButtonInfo", args);
            m_ButtonsPressed = (UIMouseButton)args[1];
            m_ButtonsUp = (UIMouseButton)args[2];
            m_ButtonsDown = (UIMouseButton)args[3];
            ReflectionUtils.SetPrivateField(@this, "m_ButtonsPressed", m_ButtonsPressed);
            ReflectionUtils.SetPrivateField(@this, "m_ButtonsUp", m_ButtonsUp);
            ReflectionUtils.SetPrivateField(@this, "m_ButtonsDown", m_ButtonsDown);

            float num = translator.GetAxis(@this.m_ScrollAxisName);
            if (!Mathf.Approximately(num, 0f))
            {
                num = Mathf.Sign(num) * Mathf.Max(1f, Mathf.Abs(num));
            }
            m_MouseMoveDelta = mousePosition - m_LastPosition;
            m_LastPosition = mousePosition;
            ReflectionUtils.SetPrivateField(@this, "m_MouseMoveDelta", m_MouseMoveDelta);
            ReflectionUtils.SetPrivateField(@this, "m_LastPosition", m_LastPosition);
            if (m_DragState == UIDragDropState.Dragging)
            {
                if (m_ButtonsUp.IsFlagSet(UIMouseButton.Left))
                {
                    if (component != null && component != m_ActiveComponent)
                    {
                        UIDragEventParameter uIDragEventParameter = CreateUIDragEventParameter(component, UIDragDropState.Dragging, m_DragData, mousePosition);
                        ReflectionUtils.InvokePrivateMethod(component, "OnDragDrop", uIDragEventParameter);
                        if (!uIDragEventParameter.used || uIDragEventParameter.state == UIDragDropState.Dragging)
                        {
                            uIDragEventParameter.Cancel();
                        }
                        uIDragEventParameter = CreateUIDragEventParameter(m_ActiveComponent, uIDragEventParameter.state, uIDragEventParameter.data, mousePosition, component);
                        ReflectionUtils.InvokePrivateMethod(m_ActiveComponent, "OnDragEnd", uIDragEventParameter);
                    }
                    else
                    {
                        UIDragDropState state = (component == null) ? UIDragDropState.CancelledNoTarget : UIDragDropState.Cancelled;
                        UIDragEventParameter p = CreateUIDragEventParameter(m_ActiveComponent, state, m_DragData, mousePosition);
                        ReflectionUtils.InvokePrivateMethod(m_ActiveComponent, "OnDragEnd", p);
                    }
                    m_DragState = UIDragDropState.None;
                    m_LastDragComponent = null;
                    m_ActiveComponent = null;
                    m_LastClickTime = 0f;
                    m_LastHoverTime = 0f;
                    m_LastPosition = mousePosition;
                    ReflectionUtils.SetPrivateField(@this, "m_DragState", m_DragState);
                    ReflectionUtils.SetPrivateField(@this, "m_LastDragComponent", m_LastDragComponent);
                    ReflectionUtils.SetPrivateField(@this, "m_ActiveComponent", m_ActiveComponent);
                    ReflectionUtils.SetPrivateField(@this, "m_LastClickTime", m_LastClickTime);
                    ReflectionUtils.SetPrivateField(@this, "m_LastHoverTime", m_LastHoverTime);
                    ReflectionUtils.SetPrivateField(@this, "m_LastPosition", m_LastPosition);
                    return;
                }
                if (component == m_ActiveComponent)
                {
                    return;
                }
                if (component != m_LastDragComponent)
                {
                    if (m_LastDragComponent != null)
                    {
                        UIDragEventParameter p2 = CreateUIDragEventParameter(m_LastDragComponent, m_DragState, m_DragData, mousePosition);
                        ReflectionUtils.InvokePrivateMethod(m_LastDragComponent, "OnDragLeave", p2);
                    }
                    if (component != null)
                    {
                        UIDragEventParameter p3 = CreateUIDragEventParameter(component, m_DragState, m_DragData, mousePosition);
                        ReflectionUtils.InvokePrivateMethod(component, "OnDragEnter", p3);
                    }
                    m_LastDragComponent = component;
                    ReflectionUtils.SetPrivateField(@this, "m_LastDragComponent", m_LastDragComponent);
                    return;
                }
                if (component != null && Vector2.Distance(mousePosition, m_LastPosition) > 1f)
                {
                    UIDragEventParameter p4 = CreateUIDragEventParameter(component, m_DragState, m_DragData, mousePosition);
                    ReflectionUtils.InvokePrivateMethod(component, "OnDragOver", p4);
                }
                return;
            }
            else if (m_ButtonsUp != UIMouseButton.None)
            {
                m_LastHoverTime = Time.realtimeSinceStartup + @this.m_HoverNotificationBegin;
                ReflectionUtils.SetPrivateField(@this, "m_LastHoverTime", m_LastHoverTime);
                if (m_ActiveComponent == null)
                {
                    ReflectionUtils.InvokePrivateMethod(@this, "SetActive", component, mousePosition, ray);
                    return;
                }
                if (m_ActiveComponent == component && m_ButtonsUp.IsFlagSet(UIMouseButton.Left))
                {
                    if (Time.realtimeSinceStartup - m_LastClickTime < @this.m_DoubleClickTime)
                    {
                        m_LastClickTime = 0f;
                        ReflectionUtils.SetPrivateField(@this, "m_LastClickTime", m_LastClickTime);
                        if (m_ActiveComponent.isEnabled)
                        {
                            ReflectionUtils.InvokePrivateMethod(m_ActiveComponent, "OnDoubleClick", new UIMouseEventParameter(m_ActiveComponent, m_ButtonsUp, 1, ray, mousePosition, Vector2.zero, num));
                        }
                    }
                    else
                    {
                        m_LastClickTime = Time.realtimeSinceStartup;
                        ReflectionUtils.SetPrivateField(@this, "m_LastClickTime", m_LastClickTime);
                        if (m_ActiveComponent.isEnabled)
                        {
                            ReflectionUtils.InvokePrivateMethod(m_ActiveComponent, "OnClick", new UIMouseEventParameter(m_ActiveComponent, m_ButtonsUp, 1, ray, mousePosition, Vector2.zero, num));
                        }
                        else
                        {
                            ReflectionUtils.InvokePrivateMethod(m_ActiveComponent, "OnDisabledClick", new UIMouseEventParameter(m_ActiveComponent, m_ButtonsUp, 1, ray, mousePosition, Vector2.zero, num));
                        }
                    }
                }
                if (m_ActiveComponent.isEnabled)
                {
                    ReflectionUtils.InvokePrivateMethod(m_ActiveComponent, "OnMouseUp", new UIMouseEventParameter(m_ActiveComponent, m_ButtonsUp, 0, ray, mousePosition, Vector2.zero, num));
                }
                if (m_ButtonsPressed == UIMouseButton.None && m_ActiveComponent != component)
                {
                    ReflectionUtils.InvokePrivateMethod(@this, "SetActive", null, mousePosition, ray);
                }
                return;
            }
            else
            {
                if (m_ButtonsDown != UIMouseButton.None)
                {
                    m_LastHoverTime = Time.realtimeSinceStartup + @this.m_HoverNotificationBegin;
                    ReflectionUtils.SetPrivateField(@this, "m_LastHoverTime", m_LastHoverTime);
                    if (m_ActiveComponent != null)
                    {
                        if (m_ActiveComponent.isEnabled)
                        {
                            ReflectionUtils.InvokePrivateMethod(m_ActiveComponent, "OnMouseDown", new UIMouseEventParameter(m_ActiveComponent, m_ButtonsDown, 0, ray, mousePosition, Vector2.zero, num));
                            return;
                        }
                    }
                    else
                    {
                        ReflectionUtils.InvokePrivateMethod(@this, "SetActive", component, mousePosition, ray);
                        if (component != null)
                        {
                            if (component.isEnabled)
                            {
                                ReflectionUtils.InvokePrivateMethod(component, "OnMouseDown", new UIMouseEventParameter(component, m_ButtonsDown, 0, ray, mousePosition, Vector2.zero, num));
                                return;
                            }
                        }
                        else if (!retainFocusSetting)
                        {
                            UIComponent activeComponent = UIView.activeComponent;
                            if (activeComponent != null)
                            {
                                activeComponent.Unfocus();
                            }
                        }
                    }
                    return;
                }
                if (m_ActiveComponent != null && m_ActiveComponent == component && m_MouseMoveDelta.magnitude == 0f && Time.realtimeSinceStartup - m_LastHoverTime > @this.m_HoverNotificationFrequency)
                {
                    ReflectionUtils.InvokePrivateMethod(m_ActiveComponent, "OnMouseHover", new UIMouseEventParameter(m_ActiveComponent, m_ButtonsPressed, 0, ray, mousePosition, Vector2.zero, num));
                    ReflectionUtils.InvokePrivateMethod(m_ActiveComponent, "OnTooltipHover", new UIMouseEventParameter(m_ActiveComponent, m_ButtonsPressed, 0, ray, mousePosition, Vector2.zero, num));
                    m_LastHoverTime = Time.realtimeSinceStartup;
                    ReflectionUtils.SetPrivateField(@this, "m_LastHoverTime", m_LastHoverTime);
                }
                if (m_ButtonsPressed == UIMouseButton.None)
                {
                    if (num != 0f && component != null)
                    {
                        ReflectionUtils.InvokePrivateMethod(@this, "SetActive", component, mousePosition, ray);
                        if (component.isEnabled)
                        {
                            ReflectionUtils.InvokePrivateMethod(component, "OnMouseWheel", new UIMouseEventParameter(component, m_ButtonsPressed, 0, ray, mousePosition, Vector2.zero, num));
                        }
                        else
                        {
                            /* *************************************************************
                             * START OF OUR "INJECTED CODE"
                             * 
                             * In order to prevent enabling scrolling on all UI controls, we have
                             * to check if the component is contained within TSContainer. This
                             * is the container where the toolbar can be found in.
                             */
                            UIComponent currentComponent = component;
                            while (currentComponent.parent != null)
                            {
                                currentComponent = currentComponent.parent;
                                if (currentComponent.name == "TSContainer")
                                {
                                    ReflectionUtils.InvokePrivateMethod(component, "OnMouseWheel", new UIMouseEventParameter(component, m_ButtonsPressed, 0, ray, mousePosition, Vector2.zero, num));
                                    break;
                                }
                            }
                            /*
                             * END OF OUR "INJECTED CODE"
                             * *************************************************************/
                        }

                        return;
                    }
                    ReflectionUtils.InvokePrivateMethod(@this, "SetActive", component, mousePosition, ray);
                }
                else if (m_ActiveComponent != null)
                {
                    if (component != null)
                    {
                        int arg_55A_0 = component.renderOrder;
                        int arg_559_0 = m_ActiveComponent.renderOrder;
                    }
                    if (m_MouseMoveDelta.magnitude >= (float)@this.m_DragStartDelta && m_ButtonsPressed.IsFlagSet(UIMouseButton.Left) && m_DragState != UIDragDropState.Denied)
                    {
                        UIDragEventParameter uIDragEventParameter2 = CreateUIDragEventParameter(m_ActiveComponent);
                        ReflectionUtils.InvokePrivateMethod(m_ActiveComponent, "OnDragStart", uIDragEventParameter2);
                        if (uIDragEventParameter2.state == UIDragDropState.Dragging)
                        {
                            m_DragState = UIDragDropState.Dragging;
                            m_DragData = uIDragEventParameter2.data;
                            ReflectionUtils.SetPrivateField(@this, "m_DragState", m_DragState);
                            ReflectionUtils.SetPrivateField(@this, "m_DragData", m_DragData);
                            return;
                        }
                        m_DragState = UIDragDropState.Denied;
                        ReflectionUtils.SetPrivateField(@this, "m_DragState", m_DragState);
                    }
                }
                if (m_ActiveComponent != null && m_MouseMoveDelta.magnitude >= 1f)
                {
                    UIMouseEventParameter p5 = new UIMouseEventParameter(m_ActiveComponent, m_ButtonsPressed, 0, ray, mousePosition, m_MouseMoveDelta, num);
                    if (m_ActiveComponent.isEnabled)
                    {
                        ReflectionUtils.InvokePrivateMethod(m_ActiveComponent, "OnMouseMove", p5);
                    }
                }
                return;
            }
        }
    }
}
