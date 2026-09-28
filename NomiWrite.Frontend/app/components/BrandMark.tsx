import Image from "next/image";

export default function BrandMark({ size = 40, className = "" }: { size?: number; className?: string }) {
  return (
    <Image
      src="/nomiwrite-mark.svg"
      alt=""
      aria-hidden="true"
      width={size}
      height={size}
      className={`block shrink-0 ${className}`}
      priority
    />
  );
}
