"use client";

import React from "react";
import "./loading.css";

interface LoadingProps {
  className?: string;
  backgroundColor?: string;
}

export function Loading({ className = "", backgroundColor = "white" }: LoadingProps) {
  const style = {
    '--loader-bg': backgroundColor,
  } as React.CSSProperties;

  return (
    <div className={`loader ${className}`} style={style}>
      <div className="box box0">
        <div></div>
      </div>
      <div className="box box1">
        <div></div>
      </div>
      <div className="box box2">
        <div></div>
      </div>
      <div className="box box3">
        <div></div>
      </div>
      <div className="box box4">
        <div></div>
      </div>
      <div className="box box5">
        <div></div>
      </div>
      <div className="box box6">
        <div></div>
      </div>
      <div className="box box7">
        <div></div>
      </div>
      <div className="ground">
        <div></div>
      </div>
    </div>
  );
}

export default Loading;
